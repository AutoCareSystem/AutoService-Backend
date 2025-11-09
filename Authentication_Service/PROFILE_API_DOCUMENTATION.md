# Customer Profile API - Implementation Documentation

**Date:** November 8, 2025  
**Branch:** feature/new-profile  
**Status:** ✅ Implementation Complete

---

## 📋 Overview

This implementation provides a complete customer profile management system that allows customers to manage their personal information and vehicles through a RESTful API.

---

## 🎯 Requirements Met

### ✅ Customer Self-Service Capabilities

1. **UserName Management**
   - ✅ Add UserName
   - ✅ Edit UserName
   
2. **Phone Number Management**
   - ✅ Add PhoneNumber
   - ✅ Edit PhoneNumber
   
3. **Address Management**
   - ✅ Add Address
   - ✅ Edit Address
   - ✅ Clear Address (set to null)
   
4. **Vehicle Management**
   - ✅ Add multiple vehicles
   - ✅ Delete vehicles
   - ❌ Edit vehicles (intentionally disabled per requirements)
   
5. **Loyalty Points**
   - ✅ View loyalty points (read-only for customers)
   - ✅ Kept in model but not user-editable

---

## 📁 Files Created/Modified

### Created Files:
1. **`Controllers/ProfileController.cs`** - Main profile API controller
2. **`profile.http`** - Comprehensive API testing file

### Modified Files:
1. **`DTOs/CustomerProfileDto.cs`** - Added new profile-specific DTOs

---

## 🔌 API Endpoints

### Customer Self-Service Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/Profile` | Get authenticated customer's profile | ✅ Customer |
| PUT | `/api/Profile` | Update authenticated customer's profile | ✅ Customer |
| POST | `/api/Profile/vehicles` | Add vehicle to authenticated customer | ✅ Customer |
| DELETE | `/api/Profile/vehicles/{vehicleId}` | Delete vehicle from authenticated customer | ✅ Customer |

### Admin/Employee Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/Profile/{userId}` | Get specific customer's profile | ✅ Admin/Employee |
| PUT | `/api/Profile/{userId}` | Update specific customer's profile | ✅ Admin/Employee |
| POST | `/api/Profile/{userId}/vehicles` | Add vehicle to specific customer | ✅ Admin/Employee |
| DELETE | `/api/Profile/{userId}/vehicles/{vehicleId}` | Delete vehicle from specific customer | ✅ Admin/Employee |

---

## 📦 DTOs (Data Transfer Objects)

### CustomerProfileResponseDto
Complete profile information returned to the client.

```csharp
{
  "userID": "string",
  "userName": "string",
  "email": "string",
  "phoneNumber": "string?",
  "address": "string?",
  "loyaltyPoints": 0,
  "createdAt": "2025-11-08T...",
  "vehicles": [
    {
      "vehicleID": 1,
      "model": "Axio",
      "year": "2017",
      "vin": "1HGBH41JXMN109186",
      "plateNumber": "CAC-4455",
      "company": "Toyota"
    }
  ]
}
```

### UpdateCustomerProfileDto
Update profile request (only editable fields).

```csharp
{
  "userName": "string?",
  "phoneNumber": "string?",
  "address": "string?"
}
```

### AddVehicleDto
Add new vehicle request.

```csharp
{
  "model": "string",
  "year": "string",
  "vin": "string",
  "plateNumber": "string",
  "company": "string?"
}
```

### VehicleDto
Vehicle information response.

```csharp
{
  "vehicleID": 1,
  "model": "string",
  "year": "string",
  "vin": "string",
  "plateNumber": "string",
  "company": "string?"
}
```

---

## 🔒 Security Features

### Authentication & Authorization
- ✅ All endpoints require JWT Bearer token authentication
- ✅ Customers can only access their own profile data
- ✅ Admin/Employee can access any customer's profile
- ✅ Role-based access control using `[Authorize(Roles = "Admin,Employee")]`

### Data Validation
- ✅ VIN uniqueness validation (prevents duplicate vehicles)
- ✅ Vehicle ownership verification (customers can only delete their own vehicles)
- ✅ Appointment check before vehicle deletion (prevents orphaned appointments)

### Token Claims Used
```csharp
ClaimTypes.NameIdentifier // User ID
ClaimTypes.Role           // User Role (Customer/Admin/Employee)
```

---

## 🚫 Intentional Limitations

1. **No Vehicle Edit Endpoint**
   - Vehicles can only be added or deleted, not edited
   - Rationale: Prevents accidental modification of critical vehicle data
   - If vehicle info needs correction, user must delete and re-add

2. **Loyalty Points Read-Only**
   - Customers can view but not modify loyalty points
   - Points can only be modified by Admin/Employee through other endpoints
   - Kept in model for display purposes

3. **Email Cannot Be Changed**
   - Email is the primary identifier and login credential
   - Changing email would require additional verification flow
   - Not included in UpdateCustomerProfileDto

---

## 🔄 Update Behavior

### Partial Updates Supported
The `PUT /api/Profile` endpoint supports partial updates. You can send only the fields you want to update:

```json
// Update only phone number
{
  "phoneNumber": "0771234567"
}

// Update only address
{
  "address": "123 New Street"
}

// Update all fields
{
  "userName": "John Doe",
  "phoneNumber": "0771234567",
  "address": "123 New Street"
}

// Clear address
{
  "address": ""
}
```

---

## ⚠️ Error Handling

### Common Error Responses

**401 Unauthorized**
```json
{
  "message": "User ID not found in token."
}
```

**403 Forbidden**
- When customer tries to access another customer's profile
- Returned automatically by ASP.NET Core authorization

**404 Not Found**
```json
{
  "message": "Customer not found."
}
```

```json
{
  "message": "Vehicle not found or does not belong to you."
}
```

**400 Bad Request**
```json
{
  "message": "A vehicle with this VIN already exists."
}
```

```json
{
  "message": "Cannot delete vehicle with existing appointments."
}
```

---

## 🧪 Testing

### Test File: `profile.http`

The provided test file includes:
- ✅ Authentication flow (register, login, get token)
- ✅ Profile retrieval (own and specific user)
- ✅ Profile updates (full and partial)
- ✅ Vehicle management (add, delete)
- ✅ Admin/Employee operations
- ✅ Error cases (unauthorized, forbidden, not found)

### How to Test:

1. **Get a Token:**
   ```http
   POST http://localhost:5093/api/Auth/login
   Content-Type: application/json

   {
     "email": "testcustomer@example.com",
     "password": "Test123!"
   }
   ```

2. **Set Variables:**
   - Copy `accessToken` from login response → `@token`
   - Extract `sub` claim from JWT → `@userId`

3. **Run Tests:**
   - Use VS Code REST Client extension
   - Or use Postman/Insomnia
   - Or use Swagger UI at `http://localhost:5093/swagger`

---

## 🔧 Database Schema

### No Changes Required
The existing database schema supports all new functionality:
- `AspNetUsers` table (UserName, Email, PhoneNumber)
- `Customers` table (Address, LoyaltyPoints)
- `Vehicles` table (Model, Year, VIN, PlateNumber, Company)

---

## 📊 Example Workflow

### Customer Journey: Complete Profile Setup

1. **Register & Login**
   ```http
   POST /api/Auth/register
   POST /api/Auth/login
   ```

2. **View Initial Profile**
   ```http
   GET /api/Profile
   // Empty profile with default email
   ```

3. **Update Personal Information**
   ```http
   PUT /api/Profile
   {
     "userName": "John Doe",
     "phoneNumber": "0771234567",
     "address": "123 Main St, Colombo"
   }
   ```

4. **Add First Vehicle**
   ```http
   POST /api/Profile/vehicles
   {
     "model": "Axio",
     "year": "2017",
     "vin": "1HGBH41JXMN109186",
     "plateNumber": "CAC-4455",
     "company": "Toyota"
   }
   ```

5. **Add Second Vehicle**
   ```http
   POST /api/Profile/vehicles
   {
     "model": "Civic",
     "year": "2020",
     "vin": "2HGFC2F59LH123456",
     "plateNumber": "ABC-1234",
     "company": "Honda"
   }
   ```

6. **View Complete Profile**
   ```http
   GET /api/Profile
   // Now includes all info and 2 vehicles
   ```

7. **Update Phone Number**
   ```http
   PUT /api/Profile
   {
     "phoneNumber": "0779876543"
   }
   ```

8. **Delete a Vehicle**
   ```http
   DELETE /api/Profile/vehicles/1
   ```

---

## 🚀 Deployment Checklist

- [ ] Code is merged to dev branch
- [ ] Database migrations are up to date (no changes needed)
- [ ] JWT authentication is configured
- [ ] CORS is configured for frontend
- [ ] Swagger documentation is accessible
- [ ] Integration tests pass
- [ ] API is documented in README

---

## 📝 Notes for Developers

### Adding New Features

**To add a new editable field:**
1. Add property to `UpdateCustomerProfileDto`
2. Update the `UpdateCustomerProfile` helper method in ProfileController
3. Update test file with new field

**To add vehicle editing (if requirements change):**
1. Create `UpdateVehicleDto` with allowed fields
2. Add `PUT /api/Profile/vehicles/{vehicleId}` endpoint
3. Verify vehicle ownership before update
4. Update documentation

### Performance Considerations

- Profile endpoint uses `.Include()` to load related data efficiently
- Vehicle queries use indexed fields (VehicleID, CustomerID, VIN)
- Consider adding pagination if customer has many vehicles

### Future Enhancements

- [ ] Vehicle photo upload
- [ ] Profile photo upload
- [ ] Email change with verification
- [ ] Phone number verification (OTP)
- [ ] Vehicle service history in profile response
- [ ] Loyalty points redemption flow

---

## 📞 Support

For questions or issues, contact the development team or create an issue in the repository.

---

**Implementation Status:** ✅ Complete and Ready for Testing
