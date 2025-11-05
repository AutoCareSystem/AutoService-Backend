# 👨‍💼 Employee Profile Management - Frontend Guide

> **For**: Frontend Developer (Profile Section)  
> **API Base**: `http://localhost:5093/api/Profile`  
> **Authentication**: Bearer Token Required

---

## 🎯 Quick Overview

Employee Profile allows employees to:

- ✅ View their profile information
- ✅ See their appointment statistics
- ✅ View recent appointments assigned to them
- ✅ Update their personal information

---

## 📊 TypeScript Interfaces

```typescript
// src/types/employee.types.ts

export interface EmployeeProfile {
  employeeID: number;
  userID: number;
  name: string;
  email: string;
  phone: string;
  position: string;
  hourlyRate: number;
  totalAppointments: number;
  completedAppointments: number;
  recentAppointments: EmployeeAppointment[];
}

export interface EmployeeAppointment {
  appointmentID: number;
  customerName: string;
  appointmentType: "Service" | "Project";
  serviceTitle: string;
  date: string; // ISO date
  time: string; // "HH:mm" format
  status: "Pending" | "Confirmed" | "InProgress" | "Completed" | "Cancelled";
  vehicleInfo?: string; // e.g., "Toyota Axio (CAC-4455)"
}

export interface UpdateEmployeeRequest {
  name?: string;
  phone?: string;
  position?: string;
}
```

---

## 🔌 API Service

```typescript
// src/services/employeeProfileService.ts
import axios from "axios";
import {
  EmployeeProfile,
  UpdateEmployeeRequest,
} from "../types/employee.types";

const API_BASE = "http://localhost:5093/api/Profile";

const api = axios.create({
  baseURL: API_BASE,
  headers: { "Content-Type": "application/json" },
});

// Add auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("authToken");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const employeeProfileService = {
  // GET employee profile
  getProfile: async (userId: number): Promise<EmployeeProfile> => {
    const response = await api.get(`/employee/${userId}`);
    return response.data;
  },

  // UPDATE employee profile
  updateProfile: async (
    userId: number,
    data: UpdateEmployeeRequest
  ): Promise<{ message: string }> => {
    const response = await api.put(`/employee/${userId}`, data);
    return response.data;
  },
};
```

---

## 🎨 React Components

### 1️⃣ Employee Profile Page

```tsx
// src/pages/EmployeeProfilePage.tsx
import React, { useEffect, useState } from "react";
import { employeeProfileService } from "../services/employeeProfileService";
import { EmployeeProfile } from "../types/employee.types";
import ProfileHeader from "../components/employee/ProfileHeader";
import StatisticsCards from "../components/employee/StatisticsCards";
import RecentAppointments from "../components/employee/RecentAppointments";
import EditProfileModal from "../components/employee/EditProfileModal";

const EmployeeProfilePage: React.FC = () => {
  const [profile, setProfile] = useState<EmployeeProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [showEdit, setShowEdit] = useState(false);

  const userId = parseInt(localStorage.getItem("userId") || "0");

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      const data = await employeeProfileService.getProfile(userId);
      setProfile(data);
    } catch (error: any) {
      console.error("Failed to load profile:", error);
      alert(error.response?.data?.message || "Failed to load profile");
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="loading">Loading...</div>;
  if (!profile) return <div className="error">Profile not found</div>;

  return (
    <div className="employee-profile-page">
      <ProfileHeader
        name={profile.name}
        position={profile.position}
        email={profile.email}
        phone={profile.phone}
        onEdit={() => setShowEdit(true)}
      />

      <StatisticsCards
        totalAppointments={profile.totalAppointments}
        completedAppointments={profile.completedAppointments}
        hourlyRate={profile.hourlyRate}
      />

      <RecentAppointments appointments={profile.recentAppointments} />

      {showEdit && (
        <EditProfileModal
          profile={profile}
          onClose={() => setShowEdit(false)}
          onUpdate={loadProfile}
        />
      )}
    </div>
  );
};

export default EmployeeProfilePage;
```

---

### 2️⃣ Profile Header Component

```tsx
// src/components/employee/ProfileHeader.tsx
import React from "react";

interface Props {
  name: string;
  position: string;
  email: string;
  phone: string;
  onEdit: () => void;
}

const ProfileHeader: React.FC<Props> = ({
  name,
  position,
  email,
  phone,
  onEdit,
}) => {
  return (
    <div className="profile-header">
      <div className="avatar-section">
        <div className="avatar">{name.charAt(0).toUpperCase()}</div>
        <div className="info">
          <h1>{name}</h1>
          <p className="position">🔧 {position}</p>
        </div>
      </div>

      <div className="contact-info">
        <div className="contact-item">
          <span className="icon">📧</span>
          <span>{email}</span>
        </div>
        <div className="contact-item">
          <span className="icon">📞</span>
          <span>{phone}</span>
        </div>
      </div>

      <button className="btn-edit" onClick={onEdit}>
        ✏️ Edit Profile
      </button>
    </div>
  );
};

export default ProfileHeader;
```

---

### 3️⃣ Statistics Cards

```tsx
// src/components/employee/StatisticsCards.tsx
import React from "react";

interface Props {
  totalAppointments: number;
  completedAppointments: number;
  hourlyRate: number;
}

const StatisticsCards: React.FC<Props> = ({
  totalAppointments,
  completedAppointments,
  hourlyRate,
}) => {
  const completionRate =
    totalAppointments > 0
      ? Math.round((completedAppointments / totalAppointments) * 100)
      : 0;

  return (
    <div className="statistics-cards">
      <div className="stat-card">
        <div className="stat-icon">📋</div>
        <div className="stat-content">
          <h3>{totalAppointments}</h3>
          <p>Total Appointments</p>
        </div>
      </div>

      <div className="stat-card">
        <div className="stat-icon">✅</div>
        <div className="stat-content">
          <h3>{completedAppointments}</h3>
          <p>Completed</p>
        </div>
      </div>

      <div className="stat-card">
        <div className="stat-icon">📊</div>
        <div className="stat-content">
          <h3>{completionRate}%</h3>
          <p>Completion Rate</p>
        </div>
      </div>

      <div className="stat-card">
        <div className="stat-icon">💰</div>
        <div className="stat-content">
          <h3>Rs. {hourlyRate.toFixed(2)}</h3>
          <p>Hourly Rate</p>
        </div>
      </div>
    </div>
  );
};

export default StatisticsCards;
```

---

### 4️⃣ Recent Appointments List

```tsx
// src/components/employee/RecentAppointments.tsx
import React from "react";
import { EmployeeAppointment } from "../../types/employee.types";

interface Props {
  appointments: EmployeeAppointment[];
}

const RecentAppointments: React.FC<Props> = ({ appointments }) => {
  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      Pending: "#FFA500",
      Confirmed: "#4CAF50",
      InProgress: "#2196F3",
      Completed: "#4CAF50",
      Cancelled: "#F44336",
    };
    return colors[status] || "#999";
  };

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  if (appointments.length === 0) {
    return (
      <div className="recent-appointments empty">
        <h2>Recent Appointments</h2>
        <p>No appointments assigned yet</p>
      </div>
    );
  }

  return (
    <div className="recent-appointments">
      <h2>Recent Appointments</h2>
      <div className="appointments-list">
        {appointments.map((apt) => (
          <div key={apt.appointmentID} className="appointment-card">
            <div className="card-header">
              <div className="customer-info">
                <h3>{apt.customerName}</h3>
                {apt.vehicleInfo && (
                  <p className="vehicle">🚗 {apt.vehicleInfo}</p>
                )}
              </div>
              <span
                className="status-badge"
                style={{ backgroundColor: getStatusColor(apt.status) }}
              >
                {apt.status}
              </span>
            </div>

            <div className="card-body">
              <div className="info-row">
                <span className="label">Service:</span>
                <span className="value">{apt.serviceTitle}</span>
              </div>
              <div className="info-row">
                <span className="label">Type:</span>
                <span className="value">{apt.appointmentType}</span>
              </div>
              <div className="info-row">
                <span className="label">Date:</span>
                <span className="value">
                  {formatDate(apt.date)} at {apt.time}
                </span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default RecentAppointments;
```

---

### 5️⃣ Edit Profile Modal

```tsx
// src/components/employee/EditProfileModal.tsx
import React, { useState } from "react";
import { employeeProfileService } from "../../services/employeeProfileService";
import {
  EmployeeProfile,
  UpdateEmployeeRequest,
} from "../../types/employee.types";

interface Props {
  profile: EmployeeProfile;
  onClose: () => void;
  onUpdate: () => void;
}

const EditProfileModal: React.FC<Props> = ({ profile, onClose, onUpdate }) => {
  const [formData, setFormData] = useState<UpdateEmployeeRequest>({
    name: profile.name,
    phone: profile.phone,
    position: profile.position,
  });
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setSaving(true);
      await employeeProfileService.updateProfile(profile.userID, formData);
      alert("Profile updated successfully!");
      onUpdate();
      onClose();
    } catch (error: any) {
      alert(error.response?.data?.message || "Failed to update profile");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Edit Profile</h2>
          <button className="btn-close" onClick={onClose}>
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Name:</label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) =>
                setFormData({ ...formData, name: e.target.value })
              }
              required
            />
          </div>

          <div className="form-group">
            <label>Phone:</label>
            <input
              type="tel"
              value={formData.phone}
              onChange={(e) =>
                setFormData({ ...formData, phone: e.target.value })
              }
              required
            />
          </div>

          <div className="form-group">
            <label>Position:</label>
            <input
              type="text"
              value={formData.position}
              onChange={(e) =>
                setFormData({ ...formData, position: e.target.value })
              }
              required
            />
          </div>

          <div className="modal-actions">
            <button type="submit" className="btn-save" disabled={saving}>
              {saving ? "Saving..." : "Save Changes"}
            </button>
            <button type="button" className="btn-cancel" onClick={onClose}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditProfileModal;
```

---

## 🎨 CSS Styling

```css
/* Employee Profile Styles */

.employee-profile-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

/* Profile Header */
.profile-header {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 20px;
}

.avatar-section {
  display: flex;
  align-items: center;
  gap: 16px;
}

.avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
  font-weight: bold;
}

.info h1 {
  margin: 0 0 8px 0;
  font-size: 24px;
  color: #333;
}

.position {
  margin: 0;
  color: #666;
  font-size: 16px;
}

.contact-info {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.contact-item {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #666;
}

.btn-edit {
  background: #2196f3;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}

.btn-edit:hover {
  background: #1976d2;
}

/* Statistics Cards */
.statistics-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-icon {
  font-size: 40px;
}

.stat-content h3 {
  margin: 0 0 4px 0;
  font-size: 28px;
  color: #333;
}

.stat-content p {
  margin: 0;
  color: #666;
  font-size: 14px;
}

/* Recent Appointments */
.recent-appointments {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.recent-appointments h2 {
  margin: 0 0 20px 0;
  color: #333;
}

.appointments-list {
  display: grid;
  gap: 16px;
}

.appointment-card {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 16px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
}

.customer-info h3 {
  margin: 0 0 4px 0;
  color: #333;
  font-size: 18px;
}

.vehicle {
  margin: 0;
  color: #666;
  font-size: 14px;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 12px;
  color: white;
  font-size: 12px;
  font-weight: 600;
}

.card-body {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.info-row {
  display: flex;
  gap: 8px;
}

.info-row .label {
  font-weight: 600;
  color: #666;
  min-width: 80px;
}

.info-row .value {
  color: #333;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 12px;
  padding: 24px;
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.btn-close {
  background: none;
  border: none;
  font-size: 32px;
  cursor: pointer;
  color: #666;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #555;
}

.form-group input {
  width: 100%;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 14px;
}

.modal-actions {
  display: flex;
  gap: 10px;
  margin-top: 24px;
}

.btn-save,
.btn-cancel {
  flex: 1;
  padding: 12px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}

.btn-save {
  background: #4caf50;
  color: white;
}

.btn-save:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.btn-cancel {
  background: #f44336;
  color: white;
}

/* Mobile Responsive */
@media (max-width: 768px) {
  .profile-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .statistics-cards {
    grid-template-columns: 1fr;
  }
}
```

---

## 📡 API Endpoints Reference

| Method | Endpoint                         | Purpose                              |
| ------ | -------------------------------- | ------------------------------------ |
| `GET`  | `/api/Profile/employee/{userId}` | Get employee profile with statistics |
| `PUT`  | `/api/Profile/employee/{userId}` | Update employee profile              |
| `GET`  | `/api/Profile/employees`         | Get all employees (Admin only)       |

---

## 📝 API Examples

### GET Employee Profile

**Request:**

```http
GET /api/Profile/employee/5
Authorization: Bearer <token>
```

**Response:**

```json
{
  "employeeID": 5,
  "userID": 5,
  "name": "John Mechanic",
  "email": "john@autoservice.com",
  "phone": "077-1234567",
  "position": "Senior Mechanic",
  "hourlyRate": 1500.0,
  "totalAppointments": 45,
  "completedAppointments": 42,
  "recentAppointments": [
    {
      "appointmentID": 101,
      "customerName": "Sakna Rajapakshe",
      "appointmentType": "Service",
      "serviceTitle": "Full Service",
      "date": "2025-11-10T00:00:00",
      "time": "10:00",
      "status": "Confirmed",
      "vehicleInfo": "Toyota Axio (CAC-4455)"
    }
  ]
}
```

### PUT Update Employee Profile

**Request:**

```http
PUT /api/Profile/employee/5
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "John Updated",
  "phone": "077-9876543",
  "position": "Lead Mechanic"
}
```

**Response:**

```json
{
  "message": "Employee profile updated successfully"
}
```

---

## ✅ Testing Checklist

- [ ] Employee can view their profile
- [ ] Statistics display correctly
- [ ] Recent appointments list shows correctly
- [ ] Status colors are accurate
- [ ] Edit modal opens and closes
- [ ] Profile updates save correctly
- [ ] Validation works on forms
- [ ] Unauthorized access is blocked
- [ ] Mobile responsive design works

---

## 🚀 Quick Integration Steps

1. Copy TypeScript interfaces
2. Create API service
3. Build components
4. Add routing
5. Test with backend at `http://localhost:5093`

---

**All Done!** Employee profile management is now complete. 🎉
