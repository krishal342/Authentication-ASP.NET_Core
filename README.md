# Authentication

A robust, enterprise-ready backend authentication and user management system built with **.NET Core**. This project implements modern security best practices, including a dual-token JWT architecture, Mailkit-powered email automation, and structured user lifecycle tracking.

---

## Tech Stack & Architecture

*   **Core Framework:** ASP.NET Core Web API
*   **Database & ORM:** PostgreSQL integrated seamlessly via Entity Framework Core (EF Core).
*   **Security & Hashing:** Custom `IPasswordHasher<User>` utilizing ASP.NET Core Identity's hashing algorithms for secure, salted-and-hashed credentials.
*   **Authentication:** Dual-token JWT system (Short-lived Access Tokens + Secure, HttpOnly Cookie Refresh Tokens).
*   **Email Engine:** Mailkit for reliable welcome notifications and transactional verification workflows.
*   **Background Processing:** Automated background workers (`IHostedService`) for proactive database optimization.

## Advanced Architectural Features

*   **Global Error Handling:** Implements a centralized `ExceptionMiddleware` to intercept errors application-wide, formatting uniform error responses and preventing internal server detail leaks.
*   **Automated Maintenance (Background Services):** 
    *   `RefreshTokenCleanupService`: Automatically purges expired refresh tokens from PostgreSQL to prevent table bloat.
    *   `OtpCleanupService`: Continuously cleanses the database of timed-out, unused One-Time Passwords (OTPs).
*   **Precise Claim Management:** Clears standard inbound claim mappings (`DefaultInboundClaimTypeMap.Clear()`) to ensure JWT claims pass exactly as defined without automatic Microsoft schema overrides.



## API Endpoint Reference

### auth route
| Endpoint | Method | Description | Payload (JSON) |
| :--- | :---: | :---: | :--- |
| `/auth/register` | `POST` | Validates inputs, hashes password, blocks duplicates, and fires email verification.  | `{ "firstName":"string", "lastName":"string", "email":"string", "password":"string" } ` |
| `/auth/verify-email` | `POST` | Check OTP and verify email while registering. | `{ "otp":"int" }` |
| `/auth/login` | `POST` | Authenticates user, issues JWT access token, sets Refresh Token in secure cookies. | `{ "email":"string", "password":"string" }`|
| `/auth/logout` | `POST` | Clears access token, deletes refresh token from cookies. | |
| `/auth/refresh` | `GET` | Reads secure refresh cookie to seamlessly issue a brand-new access token. | |
| `/auth/forget-password` | `POST` | Accepts email, generates a temporary OTP, and attaches a tracking token to the cookie. | `{ "email":"string" }` |
| `/auth/verify-otp` | `POST` | Validates submitted OTP against the verification cookie state. | `{ "opt":"int" }` |
| `/auth/reset-password` | `POST` | Reset user password. | `{ "newPassword":"string" }` |

### user route
| Endpoint | Method | Authentication | Description | Payload (JSON) |
| :--- | :---: | :---: | :---: | :--- |
| `/users` | `GET` | Required | Retrieves a paginated list of users. | |
| `/users/me` | `GET` | Required | Decodes the active context to return the logged-in user's profile/ID. | |
| `/users/{id}` | `GET` | Required | Retrieves details of a specific user by ID. | |
| `/users/{id}` | `PUT` | Required | Updates target user attributes. | `{ "firstName":"string", "lastName":"string", "email":"string", "password":"string" }` |
| `/users/{id}/change-password` | `PATCH` | Required | Changes password safely. | `{ "currentPassword":"string", "newPassword":"string" }` |
| `/users/{id}` | `DELETE` | Required | Destructive action; requires `password` re-verification to close an account. | |



