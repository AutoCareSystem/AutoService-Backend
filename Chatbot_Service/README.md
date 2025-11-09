# Chatbot Service

AI-powered chatbot service for the AutoService application that provides conversational interface for checking available appointment time slots using Google Gemini AI.

## 🚀 Features

- **AI-Powered Conversations**: Uses Google Gemini AI for natural language understanding
- **Real-Time Availability**: Checks available time slots from the database
- **Smart Slot Detection**: Automatically identifies booking-related queries
- **Flexible Time Range**: Query slots for any date range
- **Business Hours Management**: Configurable working hours (9 AM - 6 PM by default)
- **REST API**: Easy integration with frontend applications

## 📋 Prerequisites

- .NET 9.0 SDK
- PostgreSQL database (Neon or local)
- Google Gemini API key

## 🛠️ Configuration

### Environment Variables (.env)

```properties
# Database Configuration
DATABASE_URL=Host=your-host;Database=AutoService;Username=your-user;Password=your-password;SSL Mode=Require;Trust Server Certificate=true;

# Gemini AI Configuration
Gemini__ApiKey=YOUR_GEMINI_API_KEY
```

### Application Settings (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Gemini": {
    "ApiKey": ""
  }
}
```

## 🏃‍♂️ Running the Service

### Development Mode

```bash
cd Chatbot_Service
dotnet restore
dotnet run
```

The service will start on:

- HTTP: `http://localhost:5294`
- Swagger UI: `http://localhost:5294/swagger`

### Build

```bash
dotnet build
```

## 📡 API Endpoints

### 1. Health Check

```http
GET /api/Chatbot/health
```

**Response:**

```json
{
  "status": "healthy",
  "service": "Chatbot Service",
  "timestamp": "2025-11-08T10:30:00Z"
}
```

### 2. Chat with Bot

```http
POST /api/Chatbot/chat
Content-Type: application/json

{
  "message": "What are the available time slots for next week?",
  "preferredDate": "2025-11-10"
}
```

**Response:**

```json
{
  "message": "Here are the available time slots for next week: ...",
  "availableSlots": [
    {
      "date": "2025-11-10T00:00:00",
      "time": "09:00 AM",
      "isAvailable": true
    },
    {
      "date": "2025-11-10T00:00:00",
      "time": "10:00 AM",
      "isAvailable": true
    }
  ]
}
```

### 3. Get Available Slots

```http
GET /api/Chatbot/available-slots?startDate=2025-11-10&days=7
```

**Response:**

```json
[
  {
    "date": "2025-11-10T00:00:00",
    "availableSlots": [
      "09:00 AM",
      "10:00 AM",
      "11:00 AM",
      "02:00 PM",
      "03:00 PM"
    ]
  },
  {
    "date": "2025-11-11T00:00:00",
    "availableSlots": ["09:00 AM", "01:00 PM", "04:00 PM"]
  }
]
```

## 💬 Example Queries

The chatbot understands various natural language queries:

- "What are the available time slots?"
- "Show me free appointments for tomorrow"
- "When can I schedule my car service?"
- "Are there any slots available this week?"
- "Can I book an appointment on Monday?"

## 🔧 Business Logic

### Time Slot Generation

- **Working Hours**: 9:00 AM - 6:00 PM
- **Slot Duration**: 1 hour
- **Days Off**: Sundays (configurable)
- **Booking Check**: Excludes Cancelled and Rejected appointments

### Slot Availability Algorithm

1. Fetches booked appointments for the date range
2. Generates all possible time slots within business hours
3. Filters out booked slots
4. Returns available slots grouped by date

## 🧩 Project Structure

```
Chatbot_Service/
├── Controllers/
│   └── ChatbotController.cs      # API endpoints
├── Services/
│   ├── GeminiService.cs          # Google Gemini AI integration
│   └── TimeSlotService.cs        # Slot availability logic
├── Models/
│   └── Appointment.cs            # Database model
├── DTOs/
│   └── ChatDtos.cs               # Data transfer objects
├── Data/
│   └── AppDbContext.cs           # Entity Framework context
├── Properties/
│   └── launchSettings.json       # Launch configuration
├── Program.cs                     # Application entry point
├── appsettings.json              # Application settings
└── .env                          # Environment variables
```

## 🔐 Security Notes

- The `.env` file contains sensitive data and should never be committed to version control
- Always use SSL/TLS for database connections in production
- API keys should be rotated regularly
- Implement rate limiting for production use

## 🐛 Troubleshooting

### Database Connection Failed

**Issue**: `⚠️ Database connection failed!`

**Solutions**:

1. Check if DATABASE_URL is correctly set in `.env`
2. Verify database server is running
3. For Neon/Cloud databases, ensure SSL settings are correct:
   ```
   SSL Mode=Require;Trust Server Certificate=true;
   ```
4. Check firewall/network connectivity

### Gemini API Errors

**Issue**: AI responses not working

**Solutions**:

1. Verify Gemini API key is valid
2. Check API quota limits
3. Ensure internet connectivity
4. Review Gemini API status

## 📊 Database Schema

The service reads from the `Appointments` table:

```sql
CREATE TABLE "Appointments" (
    "AppointmentID" SERIAL PRIMARY KEY,
    "CustomerID" VARCHAR NOT NULL,
    "VehicleID" INTEGER NOT NULL,
    "EmployeeID" VARCHAR,
    "StartDate" TIMESTAMP NOT NULL,
    "Time" TIME NOT NULL,
    "EndDate" TIMESTAMP,
    "Status" VARCHAR(50) NOT NULL,
    "AppointmentType" VARCHAR(50) NOT NULL,
    "TotalPrice" DECIMAL(10,2)
);
```

## 🚀 Future Enhancements

- [ ] Add appointment booking capability through chat
- [ ] Support for multiple languages
- [ ] Integration with calendar systems
- [ ] SMS/Email notifications
- [ ] Chatbot conversation history
- [ ] Advanced NLP for better intent recognition
- [ ] Support for specific employee/service preferences
- [ ] Real-time slot updates via SignalR

## 📝 Testing

Use the included `Chatbot_Service.http` file for API testing with REST Client or similar tools.

### Example Test Scenarios

1. **Health Check**: Verify service is running
2. **Simple Query**: "Show available slots"
3. **Date-Specific**: "What's available tomorrow?"
4. **General Question**: "What services do you offer?"

## 📄 License

Part of the AutoService Backend project.

## 👥 Support

For issues or questions, please contact the development team.

---

**Current Status**: ✅ Fully Operational

- Database: ✅ Connected (31 appointments)
- AI Service: ✅ Configured (Gemini API)
- Port: 5294
- Environment: Development
