# Swagger API Test Cases - Phase 2 JWT Authentication

## Prerequisites
1. Start the API: `dotnet run --project TaskTracker.API`
2. Open Swagger UI: `https://localhost:7001/swagger` (or your configured port)
3. Tests should be executed in the order listed below

---

## Test Suite 1: User Registration

### Test 1.1: Register New User (Success)
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "ayush@example.com",
  "password": "Ayush123!@#",
  "confirmPassword": "Ayush123!@#",
  "firstName": "Ayush",
  "lastName": "Baunthiyal"
}
```

**Expected Response:** `201 Created`
```json
{
  "accessToken": "<JWT_TOKEN>",
  "refreshToken": "<REFRESH_TOKEN>",
  "expiresAt": "2025-11-26T...",
  "user": {
    "id": "<GUID>",
    "email": "testuser@example.com",
    "firstName": "Test",
    "lastName": "User"
  }
}
```

**Validation:**
- ✅ Status code is 201
- ✅ Response contains accessToken and refreshToken
- ✅ User object contains correct email and name
- ✅ Save the `accessToken` for subsequent tests

---

### Test 1.2: Register with Existing Email (Failure)
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "testuser@example.com",
  "password": "Test123!@#",
  "confirmPassword": "Test123!@#",
  "firstName": "Another",
  "lastName": "User"
}
```

**Expected Response:** `400 Bad Request`
```json
{
  "error": "User with this email already exists"
}
```

**Validation:**
- ✅ Status code is 400
- ✅ Error message indicates email already exists

---

## Test Suite 2: User Login

### Test 2.1: Login with Valid Credentials (Success)
**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "testuser@example.com",
  "password": "Test123!@#"
}
```

**Expected Response:** `200 OK`
```json
{
  "accessToken": "<JWT_TOKEN>",
  "refreshToken": "<REFRESH_TOKEN>",
  "expiresAt": "2025-11-26T...",
  "user": {
    "id": "<GUID>",
    "email": "testuser@example.com",
    "firstName": "Test",
    "lastName": "User"
  }
}
```

**Validation:**
- ✅ Status code is 200
- ✅ Response contains new accessToken and refreshToken
- ✅ Save the new `accessToken` and `refreshToken`

---

### Test 2.2: Login with Invalid Email (Failure)
**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "nonexistent@example.com",
  "password": "Test123!@#"
}
```

**Expected Response:** `401 Unauthorized`
```json
{
  "error": "Invalid email or password"
}
```

**Validation:**
- ✅ Status code is 401
- ✅ Generic error message (security best practice)

---

### Test 2.3: Login with Invalid Password (Failure)
**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "testuser@example.com",
  "password": "WrongPassword123!"
}
```

**Expected Response:** `401 Unauthorized`
```json
{
  "error": "Invalid email or password"
}
```

**Validation:**
- ✅ Status code is 401
- ✅ Same generic error message

---

## Test Suite 3: Token Refresh

### Test 3.1: Refresh Token with Valid Token (Success)
**Endpoint:** `POST /api/auth/refresh`

**Request Body:**
```json
{
  "refreshToken": "<REFRESH_TOKEN_FROM_LOGIN>"
}
```

**Expected Response:** `200 OK`
```json
{
  "accessToken": "<NEW_JWT_TOKEN>",
  "refreshToken": "<NEW_REFRESH_TOKEN>",
  "expiresAt": "2025-11-26T...",
  "user": {
    "id": "<GUID>",
    "email": "testuser@example.com",
    "firstName": "Test",
    "lastName": "User"
  }
}
```

**Validation:**
- ✅ Status code is 200
- ✅ New tokens are different from previous ones
- ✅ Old refresh token is now invalid (revoked)
- ✅ Save the new `accessToken` and `refreshToken`

---

### Test 3.2: Refresh Token with Invalid Token (Failure)
**Endpoint:** `POST /api/auth/refresh`

**Request Body:**
```json
{
  "refreshToken": "invalid-refresh-token-123"
}
```

**Expected Response:** `401 Unauthorized`
```json
{
  "error": "Invalid refresh token"
}
```

**Validation:**
- ✅ Status code is 401
- ✅ Error indicates invalid token

---

## Test Suite 4: Token Revocation

### Test 4.1: Revoke Refresh Token (Success)
**Endpoint:** `POST /api/auth/revoke`

**Request Body:**
```json
{
  "refreshToken": "<CURRENT_REFRESH_TOKEN>"
}
```

**Expected Response:** `204 No Content`

**Validation:**
- ✅ Status code is 204
- ✅ No response body
- ✅ Token is now revoked and cannot be used

---

### Test 4.2: Try to Use Revoked Token (Failure)
**Endpoint:** `POST /api/auth/refresh`

**Request Body:**
```json
{
  "refreshToken": "<REVOKED_REFRESH_TOKEN>"
}
```

**Expected Response:** `401 Unauthorized`
```json
{
  "error": "Invalid refresh token"
}
```

**Validation:**
- ✅ Status code is 401
- ✅ Revoked token cannot be used

---

## Test Suite 5: Protected Task Endpoints (Authorization)

### Test 5.1: Access Tasks Without Authentication (Failure)
**Endpoint:** `GET /api/tasks`

**Headers:** 
- Do NOT include Authorization header

**Expected Response:** `401 Unauthorized`

**Validation:**
- ✅ Status code is 401
- ✅ Access denied without token

---

### Test 5.2: Create Task with Valid Token (Success)
**Endpoint:** `POST /api/tasks`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN_FROM_LOGIN>
```

**Request Body:**
```json
{
  "title": "Test Task",
  "description": "This is a test task",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-12-01T10:00:00Z",
  "tags": ["test", "swagger"]
}
```

**Note:** Status: 1=Pending, 2=InProgress, 3=Completed, 4=Cancelled | Priority: 1=Low, 2=Medium, 3=High, 4=Critical

**Expected Response:** `201 Created`
```json
{
  "id": "<TASK_GUID>",
  "title": "Test Task",
  "description": "This is a test task",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-12-01T10:00:00Z",
  "tags": ["test", "swagger"],
  "userId": "<USER_GUID>",
  "createdAt": "2025-11-26T...",
  "updatedAt": "2025-11-26T..."
}
```

**Validation:**
- ✅ Status code is 201
- ✅ Task is created with authenticated user's ID
- ✅ Save the `taskId` for subsequent tests

---

### Test 5.3: Get All Tasks (Success)
**Endpoint:** `GET /api/tasks`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN>
```

**Query Parameters:**
- `pageNumber=1`
- `pageSize=10`
- `sortBy=CreatedAt`
- `sortDescending=true`

**Expected Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "<TASK_GUID>",
      "title": "Test Task",
      "description": "This is a test task",
      "status": 1,
      "priority": 2,
      "dueDate": "2025-12-01T10:00:00Z",
      "tags": ["test", "swagger"],
      "userId": "<USER_GUID>",
      "createdAt": "2025-11-26T...",
      "updatedAt": "2025-11-26T..."
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

**Validation:**
- ✅ Status code is 200
- ✅ Returns only tasks belonging to authenticated user
- ✅ Pagination metadata is correct

---

### Test 5.4: Get Task by ID (Success)
**Endpoint:** `GET /api/tasks/{taskId}`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN>
```

**Path Parameter:**
- `taskId`: `<TASK_GUID_FROM_CREATE>`

**Expected Response:** `200 OK`
```json
{
  "id": "<TASK_GUID>",
  "title": "Test Task",
  "description": "This is a test task",
  "status": 1,
  "priority": 2,
  "dueDate": "2025-12-01T10:00:00Z",
  "tags": ["test", "swagger"],
  "userId": "<USER_GUID>",
  "createdAt": "2025-11-26T...",
  "updatedAt": "2025-11-26T..."
}
```

**Validation:**
- ✅ Status code is 200
- ✅ Returns correct task details

---

### Test 5.5: Update Own Task (Success)
**Endpoint:** `PUT /api/tasks/{taskId}`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN>
```

**Request Body:**
```json
{
  "title": "Updated Test Task",
  "description": "This task has been updated",
  "status": 2,
  "priority": 3,
  "dueDate": "2025-12-05T10:00:00Z",
  "tags": ["test", "swagger", "updated"]
}
```

**Expected Response:** `200 OK`
```json
{
  "id": "<TASK_GUID>",
  "title": "Updated Test Task",
  "description": "This task has been updated",
  "status": 2,
  "priority": 3,
  "dueDate": "2025-12-05T10:00:00Z",
  "tags": ["test", "swagger", "updated"],
  "userId": "<USER_GUID>",
  "createdAt": "2025-11-26T...",
  "updatedAt": "2025-11-26T..."
}
```

**Validation:**
- ✅ Status code is 200
- ✅ Task details are updated
- ✅ `updatedAt` timestamp changed

---

### Test 5.6: Delete Own Task (Success)
**Endpoint:** `DELETE /api/tasks/{taskId}`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN>
```

**Expected Response:** `204 No Content`

**Validation:**
- ✅ Status code is 204
- ✅ Task is soft-deleted (IsDeleted = true)

---

### Test 5.7: Try to Get Deleted Task (Failure)
**Endpoint:** `GET /api/tasks/{taskId}`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN>
```

**Expected Response:** `404 Not Found`
```json
{
  "error": "Task with ID <TASK_GUID> not found"
}
```

**Validation:**
- ✅ Status code is 404
- ✅ Deleted task is not accessible

---

## Test Suite 6: Validation Tests

### Test 6.1: Register with Invalid Email Format (Failure)
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "invalid-email",
  "password": "Test123!@#",
  "confirmPassword": "Test123!@#",
  "firstName": "Test",
  "lastName": "User"
}
```

**Expected Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "Invalid email format"
    ]
  }
}
```

**Validation:**
- ✅ Status code is 400
- ✅ Validation error for email format

---

### Test 6.2: Register with Weak Password (Failure)
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "weak",
  "confirmPassword": "weak",
  "firstName": "Test",
  "lastName": "User"
}
```

**Expected Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Password": [
      "Password must be at least 8 characters long",
      "Password must contain at least one uppercase letter",
      "Password must contain at least one digit",
      "Password must contain at least one special character"
    ]
  }
}
```

**Validation:**
- ✅ Status code is 400
- ✅ Multiple password validation errors

---

### Test 6.3: Register with Mismatched Passwords (Failure)
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "Test123!@#",
  "confirmPassword": "DifferentPassword123!",
  "firstName": "Test",
  "lastName": "User"
}
```

**Expected Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "ConfirmPassword": [
      "Passwords do not match"
    ]
  }
}
```

**Validation:**
- ✅ Status code is 400
- ✅ Validation error for password mismatch

---

### Test 6.4: Create Task with Empty Title (Failure)
**Endpoint:** `POST /api/tasks`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN>
```

**Request Body:**
```json
{
  "title": "",
  "description": "Task without title",
  "status": 1,
  "priority": 2
}
```

**Expected Response:** `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": [
      "Task title is required"
    ]
  }
}
```

**Validation:**
- ✅ Status code is 400
- ✅ Validation error for empty title

---

## Test Suite 7: Authorization Tests

### Test 7.1: Register Second User
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "seconduser@example.com",
  "password": "Test123!@#",
  "confirmPassword": "Test123!@#",
  "firstName": "Second",
  "lastName": "User"
}
```

**Expected Response:** `201 Created`

**Validation:**
- ✅ Status code is 201
- ✅ Save the `accessToken` for User 2

---

### Test 7.2: User 1 Creates a Task
**Endpoint:** `POST /api/tasks`

**Headers:**
```
Authorization: Bearer <USER_1_ACCESS_TOKEN>
```

**Request Body:**
```json
{
  "title": "User 1 Task",
  "description": "This task belongs to User 1",
  "status": 1,
  "priority": 3
}
```

**Expected Response:** `201 Created`

**Validation:**
- ✅ Status code is 201
- ✅ Save `taskId` for User 1's task

---

### Test 7.3: User 2 Cannot See User 1's Task
**Endpoint:** `GET /api/tasks/{user1TaskId}`

**Headers:**
```
Authorization: Bearer <USER_2_ACCESS_TOKEN>
```

**Expected Response:** `404 Not Found`
```json
{
  "error": "Task with ID <TASK_GUID> not found"
}
```

**Validation:**
- ✅ Status code is 404
- ✅ User 2 cannot access User 1's task

---

### Test 7.4: User 2 Cannot Update User 1's Task
**Endpoint:** `PUT /api/tasks/{user1TaskId}`

**Headers:**
```
Authorization: Bearer <USER_2_ACCESS_TOKEN>
```

**Request Body:**
```json
{
  "title": "Trying to update User 1's task",
  "description": "This should fail",
  "status": 3,
  "priority": 1
}
```

**Expected Response:** `403 Forbidden`
```json
{
  "error": "You can only modify your own tasks"
}
```

**Validation:**
- ✅ Status code is 403
- ✅ Authorization prevents cross-user modifications

---

### Test 7.5: User 2 Cannot Delete User 1's Task
**Endpoint:** `DELETE /api/tasks/{user1TaskId}`

**Headers:**
```
Authorization: Bearer <USER_2_ACCESS_TOKEN>
```

**Expected Response:** `403 Forbidden`
```json
{
  "error": "You can only modify your own tasks"
}
```

**Validation:**
- ✅ Status code is 403
- ✅ Authorization prevents cross-user deletions

---

## Test Suite 8: Token Expiration

### Test 8.1: Use Expired Access Token (Optional - requires waiting)
**Note:** This test requires the access token to expire (default: 60 minutes)

**Endpoint:** `GET /api/tasks`

**Headers:**
```
Authorization: Bearer <EXPIRED_ACCESS_TOKEN>
```

**Expected Response:** `401 Unauthorized`

**Validation:**
- ✅ Status code is 401
- ✅ Expired token is rejected

---

## Test Suite 9: Edge Cases

### Test 9.1: Filter Tasks by Status
**Endpoint:** `GET /api/tasks`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN>
```

**Query Parameters:**
- `status=1` (1=Pending)
- `pageNumber=1`
- `pageSize=10`

**Expected Response:** `200 OK`

**Validation:**
- ✅ Returns only tasks with Pending status (status=1)
- ✅ Filtering works correctly

---

### Test 9.2: Filter Tasks by Priority
**Endpoint:** `GET /api/tasks`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN>
```

**Query Parameters:**
- `priority=3` (3=High)
- `pageNumber=1`
- `pageSize=10`

**Expected Response:** `200 OK`

**Validation:**
- ✅ Returns only High priority tasks (priority=3)

---

### Test 9.3: Search Tasks by Title
**Endpoint:** `GET /api/tasks`

**Headers:**
```
Authorization: Bearer <ACCESS_TOKEN>
```

**Query Parameters:**
- `searchTerm=Test`
- `pageNumber=1`
- `pageSize=10`

**Expected Response:** `200 OK`

**Validation:**
- ✅ Returns tasks containing "Test" in title or description

---

## Summary

**Total Test Cases:** 30+

**Test Coverage:**
- ✅ User Registration (3 tests)
- ✅ User Login (3 tests)
- ✅ Token Refresh (2 tests)
- ✅ Token Revocation (2 tests)
- ✅ Protected Endpoints (7 tests)
- ✅ Validation (4 tests)
- ✅ Authorization (5 tests)
- ✅ Edge Cases (3 tests)

**Testing Tips for Swagger:**
1. Execute tests in order (some depend on previous test results)
2. Copy tokens from responses and paste into subsequent requests
3. Use Swagger's "Authorize" button to set Bearer token globally
4. Check response status codes and body structure
5. Clear database between test runs for consistent results

