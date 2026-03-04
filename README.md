# Authentication

# auth route
- /auth/register
- /auth/login
- /auth/logout
- /auth/refresh -> GET request to get access token

# user route
- /users/{id} -> GET request to get user
- /users/{id} -> PUT request to update user { firstName, lastName, email, password}
- /users/{id}/change-password -> PATCH request to change password { currentPassword, newPassword}
- /users/{id} -> DELETE request to delete user { password }
