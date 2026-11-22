# AgroTransport

**Agricultural Produce Transportation Sharing & Pooling Platform**  
*Course: INT402 &bull; SIH Problem Statement: PR424 &bull; Tech: ASP.NET Core MVC + SQL Server*

---

## ⚡ How to Run

### In Visual Studio:
1. Open `AgroTransport.sln`.
2. Press **F5** (or click **Run**).
3. The database seeds automatically and opens at `http://localhost:5000`.

### In Terminal / CLI:
```bash
dotnet run --project AgroTransport/AgroTransport.csproj --urls "http://localhost:5000"
```

---

## 🔑 Demo Login Accounts

| Role | Email | Password | What to test |
| :--- | :--- | :--- | :--- |
| **Farmer** | `harpal@farmer.com` | `Farmer@123` | Search trips, pool produce space, view savings |
| **Transporter** | `gurpreet@transporter.com` | `Transporter@123` | Add vehicle, publish trip, accept/reject bookings |
| **Admin** | `admin@agrotransport.com` | `Admin@123` | View statistics, charts, users, trips, bookings |

*(The login page also has 1-click quick login buttons to fill credentials automatically.)*

---

## 🎯 2-Minute Demo Flow (What to show the Examiner)

1. **The Problem:** Farmers paying ₹10,000 to hire an entire truck for just 500–1,000 kg of produce.
2. **Step 1 (Transporter):** Log in as `gurpreet@transporter.com`.
   - Show published trip: **Phagwara to Delhi** (Eicher truck: 5,000 kg capacity, ₹10,000 cost &rarr; **₹2.00 / kg**).
3. **Step 2 (Farmer Pooling):** Log in as `harpal@farmer.com`.
   - Go to **Find Transport** &rarr; Search **Phagwara to Delhi**.
   - Show the pooling calculation:
     $$\text{Fare} = \text{Weight (kg)} \times \left(\frac{\text{Trip Cost}}{\text{Vehicle Capacity}}\right)$$
     - 1,000 kg &times; ₹2 = **₹2,000** (instead of paying ₹10,000 solo).
4. **Step 3 (Capacity Check):**
   - Transporter accepts farmer's booking &rarr; Available capacity drops from 5,000 kg to 4,000 kg.
   - If farmer cancels &rarr; Capacity is automatically restored to 5,000 kg.
5. **Step 4 (Admin Dashboard):** Log in as `admin@agrotransport.com`.
   - Show the summary cards, Chart.js graphs, user list, and bookings audit.

---

## 🚀 Key Features

- **Role-based Portals:** Separate dashboards for Farmer, Transporter, and Admin.
- **Vehicle Fleet Management:** CRUD for Mini Trucks, Pickup Trucks, Tractor Trolleys, etc.
- **Trip Scheduling:** Transporters set route, departure date/time, and charter price.
- **Transport Search:** Search trips by pickup location, destination, date, and load capacity.
- **Proportional Fare Pooling:** Multiple farmers share one vehicle and split the cost accurately.
- **Overbooking Prevention:** Concurrency checks and DB transactions prevent double-booking.
- **In-App Notifications:** Real notifications in SQL Server for booking events.
- **RESTful APIs:** CRUD endpoints for `/api/vehiclesapi`, `/api/tripsapi`, `/api/bookingsapi`.
- **SQL Views & Stored Procedures:** `vw_ActiveTransportationTrips`, `vw_TripBookingSummary`, and `usp_GetAvailableTripsByRoute`.

---

## 🎓 Top 5 Viva Questions & Answers

### 1. What is the core logic of transportation pooling in your project?
Instead of one farmer hiring a full vehicle, multiple farmers share the space. The system calculates:
$$\text{Cost Per Kg} = \frac{\text{Total Trip Cost}}{\text{Total Capacity}}$$
Each farmer pays: $\text{Requested Quantity} \times \text{Cost Per Kg}$. All calculations happen on the server to prevent tampering.

### 2. How did you prevent overbooking when multiple farmers book at once?
Using **database transactions** (`BeginTransactionAsync`) and **optimistic concurrency control** (`[ConcurrencyCheck] Guid ConcurrencyStamp` on the trip entity). If two users book at the same second, the second transaction is rolled back safely.

### 3. Which architecture and technologies are used?
- **Pattern:** ASP.NET Core MVC (Model-View-Controller)
- **Language:** C# 12
- **ORM:** Entity Framework Core (Code-First with Migrations)
- **Database:** Microsoft SQL Server (LocalDB)
- **Security:** ASP.NET Core Identity with Role-Based Access Control (RBAC)

### 4. What is the difference between `ViewBag`, `ViewData`, and `TempData`?
- `ViewData`: Key-value dictionary valid for the current request only.
- `ViewBag`: Dynamic wrapper around `ViewData`.
- `TempData`: Persists data across redirects (used for success/error flash messages).

### 5. What are the roles in this system?
- **Farmer:** Searches trips, pools space, tracks bookings, creates transport requests.
- **Transporter:** Manages vehicles, publishes trips, accepts/declines bookings.
- **Administrator:** Monitors platform statistics, manages users, and audits trips.
