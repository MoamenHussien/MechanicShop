# MechanicShop System

## Configuration

To configure and run the MechanicShop infrastructure and application:

1. Copy `.env.example` to `.env` in the project root:
   ```bash
   cp .env.example .env
   ```
2. Fill in the required secret environment variable values inside `.env`:
   - `SA_PASSWORD`
   - `JWT_SECRET_KEY`
   - `MAIL_USERNAME`
   - `MAIL_PASSWORD`
   - `GF_SECURITY_ADMIN_PASSWORD`
3. Build and launch the container environment:
   ```bash
   docker compose up --build
   ```