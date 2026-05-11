import urllib.request
import urllib.error
import json
import sys
import io
import base64
import time

RUN_ID = str(int(time.time()))[-6:]  # last 6 digits of epoch — unique per run

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

BASE = "http://localhost:8080/api"
results = []

def req(method, path, body=None, token=None):
    url = BASE + path
    data = json.dumps(body).encode() if body is not None else None
    headers = {"Content-Type": "application/json", "Accept": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    r = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(r) as resp:
            raw = resp.read().decode()
            return resp.status, json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        raw = e.read().decode()
        try:
            return e.code, json.loads(raw)
        except:
            return e.code, raw

def check(label, status, body, expect_status, expect_fn=None):
    ok = status == expect_status
    if ok and expect_fn:
        ok = expect_fn(body)
    icon = "PASS" if ok else "FAIL"
    results.append((icon, label, status, expect_status))
    print(f"  [{icon}] {label} -- HTTP {status} (expected {expect_status})")
    if not ok:
        print(f"         body: {str(body)[:300]}")
    return ok, body

def decode_jwt_user_id(token):
    """Extract nameid (user ID) from JWT payload."""
    try:
        payload_b64 = token.split(".")[1]
        # Add padding
        payload_b64 += "=" * (-len(payload_b64) % 4)
        payload = json.loads(base64.b64decode(payload_b64).decode())
        return payload.get("nameid")
    except Exception:
        return None

def login(username, password):
    s, b = req("POST", "/auth/login", {"username": username, "password": password})
    if isinstance(b, dict):
        token = b.get("token") or b.get("data", {}).get("token")
        if token:
            user_id = decode_jwt_user_id(token)
            return token, user_id
    return None, None

# ── SETUP ────────────────────────────────────────────────────────────────────
print("\n=== SETUP: Create Users ===")

ADMIN_USER = f"adm{RUN_ID}"
CUSTOMER_USER = f"cust{RUN_ID}"
ADMIN_EMAIL = f"adm{RUN_ID}@ambev.com"
CUSTOMER_EMAIL = f"cust{RUN_ID}@ambev.com"

admin_payload = {
    "email": ADMIN_EMAIL,
    "username": ADMIN_USER,
    "password": "Admin@123456",
    "name": {"firstname": "Test", "lastname": "Admin"},
    "address": {
        "city": "Sao Paulo", "street": "Av Paulista", "number": 1,
        "zipcode": "01310-100",
        "geolocation": {"lat": "-23.56", "long": "-46.65"}
    },
    "phone": "+5511999990001",
    "status": 1,
    "role": 3
}
s, b = req("POST", "/users", admin_payload)
print(f"  Create admin user -> HTTP {s}")

customer_payload = {
    "email": CUSTOMER_EMAIL,
    "username": CUSTOMER_USER,
    "password": "Customer@123456",
    "name": {"firstname": "Test", "lastname": "Customer"},
    "address": {
        "city": "Rio de Janeiro", "street": "Rua da Praia", "number": 10,
        "zipcode": "20040-020",
        "geolocation": {"lat": "-22.90", "long": "-43.17"}
    },
    "phone": "+5521999990002",
    "status": 1,
    "role": 1
}
s, b = req("POST", "/users", customer_payload)
print(f"  Create customer user -> HTTP {s}")

# ── AUTH: Login ───────────────────────────────────────────────────────────────
print("\n=== AUTH: Login ===")
admin_token, admin_id = login(ADMIN_USER, "Admin@123456")
ok_login = admin_token is not None
results.append(("PASS" if ok_login else "FAIL", "POST /api/auth/login", 200 if ok_login else 0, 200))
print(f"  [{'PASS' if ok_login else 'FAIL'}] POST /api/auth/login -- HTTP {'200' if ok_login else 'ERR'} (expected 200)")
if not admin_token:
    print("  FATAL: cannot obtain admin token -- aborting")
    sys.exit(1)
print(f"  Admin token obtained, user ID: {admin_id}")

# Get customer ID
customer_token, customer_id = login(CUSTOMER_USER, "Customer@123456")
print(f"  Customer login -> {'OK' if customer_token else 'FAIL'}, user ID: {customer_id}")

# Use admin token for protected endpoints
token = admin_token
# Use customer_id if available, else fall back to admin_id
cart_user_id = customer_id or admin_id
sale_customer_id = customer_id or admin_id

# ── USERS ─────────────────────────────────────────────────────────────────────
print("\n=== USERS ===")

s, b = req("GET", "/users", token=token)
check("GET /api/users (list)", s, b, 200)

if admin_id:
    s, b = req("GET", f"/users/{admin_id}", token=token)
    check("GET /api/users/{id}", s, b, 200)

    update_payload = {
        "username": ADMIN_USER,
        "email": ADMIN_EMAIL,
        "phone": "+5511999990001",
        "firstName": "Updated",
        "lastName": "Admin",
        "city": "Sao Paulo",
        "street": "Av Paulista",
        "number": 1,
        "zipCode": "01310-100",
        "geoLat": "-23.56",
        "geoLong": "-46.65",
        "role": 3,
        "status": 1
    }
    s, b = req("PUT", f"/users/{admin_id}", update_payload, token=token)
    check("PUT /api/users/{id}", s, b, 200)

    patch_payload = {"role": 3, "status": 1}
    s, b = req("PATCH", f"/users/{admin_id}/role", patch_payload, token=token)
    check("PATCH /api/users/{id}/role", s, b, 200)
else:
    results.append(("SKIP", "GET /api/users/{id}", 0, 200))
    results.append(("SKIP", "PUT /api/users/{id}", 0, 200))
    results.append(("SKIP", "PATCH /api/users/{id}/role", 0, 200))
    print("  [SKIP] User-specific tests -- no admin_id")

# ── PRODUCTS ──────────────────────────────────────────────────────────────────
print("\n=== PRODUCTS ===")

prod_payload = {
    "title": "Test Product",
    "price": 99.99,
    "description": "A test product for API testing",
    "category": "electronics",
    "image": "https://fakestoreapi.com/img/test.jpg",
    "rating": {"rate": 4.5, "count": 100}
}
s, b = req("POST", "/products", prod_payload, token=token)
check("POST /api/products", s, b, 201)
product_id = b.get("data", {}).get("id") if isinstance(b, dict) else None
print(f"  Created product id={product_id}")

s, b = req("GET", "/products", token=token)
check("GET /api/products (list)", s, b, 200)

s, b = req("GET", "/products?_page=1&_size=5", token=token)
check("GET /api/products (paginated)", s, b, 200)

s, b = req("GET", "/products/categories", token=token)
check("GET /api/products/categories", s, b, 200)

s, b = req("GET", "/products/category/electronics", token=token)
check("GET /api/products/category/{category}", s, b, 200)

if product_id:
    s, b = req("GET", f"/products/{product_id}", token=token)
    check("GET /api/products/{id}", s, b, 200)

    prod_update = {
        "title": "Updated Test Product",
        "price": 149.99,
        "description": "Updated description",
        "category": "electronics",
        "image": "https://fakestoreapi.com/img/test.jpg",
        "rating": {"rate": 4.8, "count": 120}
    }
    s, b = req("PUT", f"/products/{product_id}", prod_update, token=token)
    check("PUT /api/products/{id}", s, b, 200)

    s, b = req("DELETE", f"/products/{product_id}", token=token)
    check("DELETE /api/products/{id}", s, b, 200)

# ── CARTS ─────────────────────────────────────────────────────────────────────
print("\n=== CARTS ===")

# Create a product for cart tests
s, b = req("POST", "/products", {
    "title": "Cart Product",
    "price": 50.00,
    "description": "Product for cart testing",
    "category": "test-category",
    "image": "https://example.com/img.jpg",
    "rating": {"rate": 3.0, "count": 10}
}, token=token)
cart_product_id = b.get("data", {}).get("id") if isinstance(b, dict) else None
print(f"  Cart product id={cart_product_id}")

if cart_user_id and cart_product_id:
    cart_payload = {
        "userId": cart_user_id,
        "date": "2026-01-15T00:00:00Z",
        "products": [
            {"productId": cart_product_id, "quantity": 2}
        ]
    }
    s, b = req("POST", "/carts", cart_payload, token=token)
    check("POST /api/carts", s, b, 201)
    cart_id = b.get("data", {}).get("id") if isinstance(b, dict) else None
    print(f"  Created cart id={cart_id}")
else:
    results.append(("SKIP", "POST /api/carts", 0, 201))
    print("  [SKIP] POST /api/carts -- missing userId or productId")
    cart_id = None

s, b = req("GET", "/carts", token=token)
check("GET /api/carts (list)", s, b, 200)

if cart_id:
    s, b = req("GET", f"/carts/{cart_id}", token=token)
    check("GET /api/carts/{id}", s, b, 200)

    cart_update = {
        "userId": cart_user_id,
        "date": "2026-02-01T00:00:00Z",
        "products": [
            {"productId": cart_product_id, "quantity": 5}
        ]
    }
    s, b = req("PUT", f"/carts/{cart_id}", cart_update, token=token)
    check("PUT /api/carts/{id}", s, b, 200)

    s, b = req("DELETE", f"/carts/{cart_id}", token=token)
    check("DELETE /api/carts/{id}", s, b, 200)
else:
    results.append(("SKIP", "GET /api/carts/{id}", 0, 200))
    results.append(("SKIP", "PUT /api/carts/{id}", 0, 200))
    results.append(("SKIP", "DELETE /api/carts/{id}", 0, 200))
    print("  [SKIP] Cart CRUD tests -- no cart_id")

# ── SALES ─────────────────────────────────────────────────────────────────────
print("\n=== SALES ===")

# Create a product for sale tests
s, b = req("POST", "/products", {
    "title": "Sale Product A",
    "price": 200.00,
    "description": "Product for sale testing",
    "category": "beverages",
    "image": "https://example.com/img.jpg",
    "rating": {"rate": 4.0, "count": 50}
}, token=token)
sale_product_id = b.get("data", {}).get("id") if isinstance(b, dict) else None
print(f"  Sale product id={sale_product_id}")

if sale_customer_id and sale_product_id:
    sale_payload = {
        "saleNumber": f"SALE-{RUN_ID}-001",
        "saleDate": "2026-05-11T00:00:00Z",
        "customerId": sale_customer_id,
        "customerName": "Test Customer",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "branchName": "Branch SP",
        "items": [
            {
                "productId": sale_product_id,
                "productName": "Sale Product A",
                "quantity": 5,
                "unitPrice": 200.00
            }
        ]
    }
    s, b = req("POST", "/sales", sale_payload, token=token)
    check("POST /api/sales", s, b, 201)
    sale_id = b.get("data", {}).get("id") if isinstance(b, dict) else None
    print(f"  Created sale id={sale_id}")
else:
    results.append(("SKIP", "POST /api/sales", 0, 201))
    print("  [SKIP] POST /api/sales -- missing customerId or productId")
    sale_id = None

s, b = req("GET", "/sales", token=token)
check("GET /api/sales (list)", s, b, 200)

s, b = req("GET", "/sales?_page=1&_size=5", token=token)
check("GET /api/sales (paginated)", s, b, 200)

if sale_id:
    s, b = req("GET", f"/sales/{sale_id}", token=token)
    check("GET /api/sales/{id}", s, b, 200)

    s, b = req("GET", f"/sales?saleNumber=SALE-{RUN_ID}-001", token=token)
    check("GET /api/sales?saleNumber={num}", s, b, 200)

    update_sale_payload = {
        "saleNumber": f"SALE-{RUN_ID}-001-UPD",
        "saleDate": "2026-05-11T00:00:00Z",
        "customerId": sale_customer_id,
        "customerName": "Test Customer Updated",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "branchName": "Branch SP Updated",
        "items": [
            {
                "productId": sale_product_id,
                "productName": "Sale Product A",
                "quantity": 3,
                "unitPrice": 200.00
            }
        ]
    }
    s, b = req("PUT", f"/sales/{sale_id}", update_sale_payload, token=token)
    check("PUT /api/sales/{id}", s, b, 200)

    # Create a second sale specifically for cancel-item + delete tests
    s2, b2 = req("POST", "/sales", {
        "saleNumber": f"SALE-{RUN_ID}-CANCEL",
        "saleDate": "2026-05-11T00:00:00Z",
        "customerId": sale_customer_id,
        "customerName": "Test Customer",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "branchName": "Branch SP",
        "items": [
            {
                "productId": sale_product_id,
                "productName": "Sale Product A",
                "quantity": 4,
                "unitPrice": 200.00
            }
        ]
    }, token=token)
    cancel_sale_id = b2.get("data", {}).get("id") if isinstance(b2, dict) else None

    if cancel_sale_id:
        s3, b3 = req("GET", f"/sales/{cancel_sale_id}", token=token)
        cancel_item_id = None
        if s3 == 200 and isinstance(b3, dict):
            items = b3.get("data", {}).get("items", [])
            if items:
                cancel_item_id = items[0].get("id")
        print(f"  Cancel sale id={cancel_sale_id}, item id={cancel_item_id}")

        if cancel_item_id:
            s, b = req("PATCH", f"/sales/{cancel_sale_id}/items/{cancel_item_id}/cancel", token=token)
            check("PATCH /api/sales/{id}/items/{itemId}/cancel", s, b, 200)
        else:
            results.append(("SKIP", "PATCH /api/sales/{id}/items/{itemId}/cancel", 0, 200))
            print("  [SKIP] cancel item -- no item ID found")

        s, b = req("DELETE", f"/sales/{cancel_sale_id}", token=token)
        check("DELETE /api/sales/{id}", s, b, 200)
    else:
        results.append(("SKIP", "PATCH /api/sales/{id}/items/{itemId}/cancel", 0, 200))
        results.append(("SKIP", "DELETE /api/sales/{id}", 0, 200))
        print("  [SKIP] cancel + delete sale tests -- second sale creation failed")
else:
    for lbl in ["GET /api/sales/{id}", "GET /api/sales?saleNumber={num}",
                "PUT /api/sales/{id}", "PATCH /api/sales/{id}/items/{itemId}/cancel",
                "DELETE /api/sales/{id}"]:
        results.append(("SKIP", lbl, 0, 200))
    print("  [SKIP] Sale CRUD tests -- no sale_id")

# ── 401 UNAUTHORIZED ──────────────────────────────────────────────────────────
print("\n=== 401 UNAUTHORIZED (no token) ===")
s, b = req("GET", "/users")
check("GET /api/users without token -> 401", s, b, 401)

s, b = req("GET", "/products")
check("GET /api/products without token -> 401", s, b, 401)

s, b = req("GET", "/carts")
check("GET /api/carts without token -> 401", s, b, 401)

s, b = req("GET", "/sales")
check("GET /api/sales without token -> 401", s, b, 401)

# ── CLEANUP ───────────────────────────────────────────────────────────────────
print("\n=== CLEANUP ===")
if customer_id:
    s, b = req("DELETE", f"/users/{customer_id}", token=token)
    check("DELETE /api/users/{id} (customer)", s, b, 200)
if admin_id:
    s, b = req("DELETE", f"/users/{admin_id}", token=token)
    check("DELETE /api/users/{id} (admin)", s, b, 200)

# ── FINAL REPORT ─────────────────────────────────────────────────────────────
print("\n" + "=" * 60)
print("FINAL REPORT")
print("=" * 60)
passed = [r for r in results if r[0] == "PASS"]
failed = [r for r in results if r[0] == "FAIL"]
skipped = [r for r in results if r[0] == "SKIP"]

for icon, label, got, expected in results:
    sym = "[PASS]" if icon == "PASS" else ("[SKIP]" if icon == "SKIP" else "[FAIL]")
    print(f"  {sym} {label}")

print(f"\n  Total: {len(results)} | Passed: {len(passed)} | Failed: {len(failed)} | Skipped: {len(skipped)}")
if failed:
    print("\n  Failed tests:")
    for _, label, got, expected in failed:
        print(f"    [FAIL] {label} (got HTTP {got}, expected {expected})")
