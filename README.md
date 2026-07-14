# electronic-payment-system
Master project - Vehicle rental agency payment system

A secure, distributed, and highly available full-stack system designed to facilitate vehicle rentals integrated with a robust, microservice-inspired Payment Service Provider (PSP). The architecture is built with a **C# (.NET)** backend and a **Vue.js** frontend, prioritizing loose coupling, security compliance, and high availability.

## 🏗️ System Architecture Overview

The system consists of three completely decoupled environments communicating via secure protocols:
1.  **Vehicle Rental Web Shop:** An e-commerce platform where registered users browse vehicles, select insurance packages, and initiate rentals.
2.  **Payment Service Provider (PSP):** A pluggable, multi-tenant hub managed by a super-admin. It acts as an intermediary gateway between external merchant shops and various payment processing nodes.
3.  **Acquirer Bank / Payment Mock Engines:** Standalone service simulators representing the Bank API, PayPal Sandbox, and a Crypto Testnet Wallet.

### Core Architecture Principles:
*   **High Availability (HA):** Engineered to scale horizontally across multiple nodes (minimum 2 machines/cloud environments) without downtime during runtime integrations.
*   **Pluggable Design:** Payment methods are implemented as independent modules/plugins, making it easy to seamlessly introduce new providers (e.g., Payoneer).
*   **Secure Protocols:** Mandatory use of strict secure communication layers (`HTTPS` and encrypted `gRPCs`).

---

## 🛠️ Tech Stack

*   **Backend:** C# (.NET Core / Web API), Entity Framework Core
*   **Frontend:** Vue.js, Axios
*   **Database:** PostgreSQL 
*   **Communication:** REST APIs (HTTPS), gRPCs (Secure Remote Procedure Calls)

---

## 💳 Supported Payment Methods (PSP Plugins)

### 1. Bank Payment Gateway
*   **Credit/Debit Cards:** Integrates with the Acquirer Bank API using secure merchant authentication (`MERCHANT_ID`, API keys/HMAC tokens). Features a time-limited, single-attempt checkout page with front-end **Luhn Algorithm** validation for credit card PANs.
*   **IPS QR Code:** Generates and validates dynamic QR codes compliant with the **NBS (National Bank of Serbia) IPS "Skeniraj & Plati"** standard, utilizing real-time digital scanning frames.

### 2. PayPal Integration
*   Facilitates merchant-client fund transfers by consuming the official **PayPal REST API** sandbox environment, securely handling state synchronization between the web shop and PSP.

### 3. Cryptocurrency Gateway
*   Processes decentralized real-time transactions over a standard **Bitcoin Testnet** (or alternative testnet API) mapping fiat prices to crypto amounts dynamically at checkout.

---

## 🔒 Security & PCI DSS Compliance

Given the financial nature of the PSP, the system implements stringent technical controls aligned with the **PCI DSS** framework:
*   **Protect Account Data (Req. 2):** Complete enforcement of data protection. Sensitive credit card metadata is never stored in plain text; strong encryption and tokenization are enforced at rest.
*   **Strong Access Control (Req. 4):** Role-Based Access Control (RBAC) ensuring super-admins, merchants, and clients operate strictly under the Principle of Least Privilege.
*   **Tracking & Monitoring (Req. 5.1):** Centralized, immutable audit logs tracking all network access, database modifications, and access attempts to cardholder data environments.

---

## ⚙️ Transaction Integrity & Edge-Case Handling

The PSP core engine is robustly engineered to manage real-time distributed failures:
*   **Price Tampering Prevention:** Strict server-side validation blocks any unauthorized amount modifications mid-checkout.
*   **Idempotency & Double-Click Protection:** Implements unique transaction tokens to instantly reject duplicate requests if a user double-clicks the payment button.
*   **Polled Status Checking:** Dedicated background polling workers safely verify and fetch final transaction statuses if a connection drops between the Bank, PSP, and Web Shop.
*   **Session Abandonment:** Automatic timeout management and webhooks to clean up and release pending vehicle holds if a user closes their browser tab mid-transaction.

---

### 👥 Authors & Roles
*   Katarina Petrović 
*   Nenad Gvozdenac
*   Miloš Vijuk
