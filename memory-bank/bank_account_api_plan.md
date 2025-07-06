# BankAccount API Management Implementation Plan

## Overview
Create comprehensive BankAccount management APIs within the existing PaymentService to handle bank account operations for museums.

## Implementation Steps

### 1. DTOs Creation ✅ COMPLETED
Created BankAccount-related DTOs in `src/Application/DTOs/Payment/`:
- `BankAccountDto`: Display data ✅
- `BankAccountCreateDto`: Creation data ✅
- `BankAccountUpdateDto`: Update data ✅
- `BankAccountQuery`: Query parameters for filtering/pagination ✅
- `BankAccountProfile`: AutoMapper profile ✅

### 2. Repository Implementation ✅ COMPLETED
Created `BankAccountRepository` in `src/Infrastructure/Repository/`:
- Follow existing repository pattern ✅
- Implement CRUD operations ✅
- Include museum-specific queries ✅
- Handle pagination and filtering ✅
- Account number uniqueness validation ✅

### 3. Service Methods Addition ✅ COMPLETED
Added BankAccount management methods to existing `PaymentService`:
- `HandleGetBankAccountsByMuseum`: Get all bank accounts for a museum ✅
- `HandleGetBankAccountById`: Get specific bank account ✅
- `HandleCreateBankAccount`: Create new bank account ✅
- `HandleUpdateBankAccount`: Update existing bank account ✅
- `HandleDeleteBankAccount`: Delete bank account ✅

### 4. API Endpoints
Add endpoints to existing PaymentController or create dedicated routes:
- `GET /api/v1/museums/{museumId}/bank-accounts`: List museum's bank accounts
- `GET /api/v1/bank-accounts/{id}`: Get specific bank account
- `POST /api/v1/museums/{museumId}/bank-accounts`: Create bank account
- `PUT /api/v1/bank-accounts/{id}`: Update bank account
- `DELETE /api/v1/bank-accounts/{id}`: Delete bank account

### 5. Security & Validation
- Protected endpoints requiring authentication
- Museum ownership validation
- Admin privileges for certain operations
- Input validation and sanitization

## Technical Requirements
- Follow existing service/repository patterns
- Use AutoMapper for DTO mapping
- Implement proper error handling
- Include comprehensive logging
- Follow API conventions established in memory-bank/api_conventions.md

## Files to Create/Modify
1. `src/Application/DTOs/Payment/BankAccountDto.cs` (New)
2. `src/Infrastructure/Repository/BankAccountRepository.cs` (New) 
3. `src/Application/Service/PaymentService.cs` (Modify)
4. Add controller methods (if needed)

## Validation Rules
- HolderName: Required, max 100 characters
- BankName: Required, max 100 characters  
- AccountNumber: Required, max 100 characters, unique per museum
- QRCode: Required, max 1000 characters
- MuseumId: Required, must exist

## Implementation Summary

✅ **COMPLETED**: BankAccount API management has been successfully implemented in PaymentService.cs

### Files Created:
1. `src/Application/DTOs/Payment/BankAccountDto.cs` - All DTOs and query classes
2. `src/Application/DTOs/Payment/BankAccountProfile.cs` - AutoMapper profile
3. `src/Infrastructure/Repository/BankAccountRepository.cs` - Complete repository implementation

### Files Modified:
1. `src/Application/Service/PaymentService.cs` - Added BankAccount management methods

### Features Implemented:
- **CRUD Operations**: Full Create, Read, Update, Delete functionality
- **Museum-specific Access**: Bank accounts are tied to specific museums
- **Search & Pagination**: Query support with search functionality
- **Account Number Validation**: Ensures uniqueness per museum
- **Authentication**: All endpoints require valid JWT token
- **Error Handling**: Comprehensive error responses
- **AutoMapper Integration**: Proper DTO mapping

### Ready for Controller Integration:
The service methods are ready to be called from any controller following the pattern:
```csharp
[HttpGet("museums/{museumId}/bank-accounts")]
public async Task<IActionResult> GetBankAccountsByMuseum(Guid museumId, [FromQuery] BankAccountQuery query)
{
    return await _paymentService.HandleGetBankAccountsByMuseum(museumId, query);
}
``` 