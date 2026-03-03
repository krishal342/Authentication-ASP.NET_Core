# Authentication

# auth route
- /auth/register
- /auth/login
- /auth/logout

# user route
- /users/{id} -> GET request to get user
- /users/{id} -> PUT request to update user { firstName, lastName, email, password}
- /users/{id} -> PATCH request to change password { currentPassword, newPassword}
- /users/{id} -> DELETE request to delete user { password }