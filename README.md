# Authentication

# auth route
- /auth/register
- /auth/login
- /auth/logout
- /auth/refresh -> GET request to get access token

- /forget-password 
- -> POST request to send OTP, from body { email }

- /validate-otp 
- -> POST request to validate OTP, from body { opt }, response reset token if validate

- /reset-password
- -> POST request to reset password, from body { email, newPassword }, reset token in authentication header 

# user route
- /users/{id} -> GET request to get user
- /users/{id} -> PUT request to update user { firstName, lastName, email, password}
- /users/{id}/change-password -> PATCH request to change password { currentPassword, newPassword}
- /users/{id} -> DELETE request to delete user { password }


