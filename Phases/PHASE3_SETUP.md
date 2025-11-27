# Task Tracker - Phase 3 Setup Guide

## Prerequisites

- Node.js 18+ and npm
- .NET 9.0 SDK
- PostgreSQL (via Docker)
- Git

## Quick Start

### 1. Start PostgreSQL Database

```powershell
cd d:\repos\mygit\TaskTrackerApp
docker-compose up -d
```

This will start PostgreSQL on port 5433.

### 2. Start Backend API

```powershell
cd d:\repos\mygit\TaskTrackerApp\TaskTracker.API
dotnet run
```

The API will be available at:
- http://localhost:5128
- Swagger UI: http://localhost:5128/swagger

### 3. Install and Start React UI

```powershell
cd d:\repos\mygit\TaskTrackerApp\task-tracker-ui
npm install
npm run dev
```

The UI will be available at http://localhost:3000

## First Time Setup

### Install UI Dependencies

```powershell
cd task-tracker-ui
npm install
```

This will install all required packages:
- React and React DOM
- React Router DOM
- Axios
- React Hot Toast
- date-fns
- lucide-react
- Tailwind CSS
- TypeScript
- Vite

## Testing the Application

### 1. Register a New User

1. Open http://localhost:3000
2. Click "Sign up" link
3. Fill in the registration form:
   - First Name: John
   - Last Name: Doe
   - Email: john@example.com
   - Password: Password123!
4. Click "Sign Up"

### 2. Create Tasks

1. After login, you'll be on the Tasks page
2. Click "New Task" button
3. Fill in task details:
   - Title: "Complete Phase 3"
   - Description: "Finish React UI implementation"
   - Status: In Progress
   - Priority: High
   - Due Date: (tomorrow's date)
   - Tags: "development", "urgent"
4. Click "Create Task"

### 3. Test Search and Filters

1. Use the search box to find tasks
2. Click "Filters" to show filter options
3. Filter by Status, Priority
4. Sort by different fields

### 4. Test Due Date Alerts

1. Create a task with due date within 24 hours
2. The task card will have a yellow border
3. An alert icon will show "Due within 24 hours"

### 5. Test Authorization

1. Tasks created by you can be edited/deleted
2. You can view all tasks (public read)
3. Trying to delete another user's task will show error message

## Environment Configuration

### Development (.env.development)
```
VITE_API_BASE_URL=http://localhost:5128/api
```

### Production (.env.production)
```
VITE_API_BASE_URL=http://tasktracker-api:5128/api
```

## Docker Deployment (Future)

When ready to deploy all services together:

1. Uncomment API and UI services in `docker-compose.yml`
2. Create Dockerfile for API (if not exists)
3. Run:

```powershell
docker-compose up --build
```

This will start:
- PostgreSQL on port 5433
- API on port 5128
- UI on port 3000

## Troubleshooting

### API Connection Issues

**Problem:** UI can't connect to API
**Solution:** 
- Ensure API is running on port 5128
- Check CORS settings in API Program.cs
- Verify .env.development has correct API URL

### Build Errors

**Problem:** TypeScript errors
**Solution:**
```powershell
npm install
npm run dev
```

**Problem:** Tailwind CSS not working
**Solution:**
```powershell
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

### Authentication Issues

**Problem:** Token expired
**Solution:** The app automatically refreshes tokens. If persistent, clear localStorage and login again.

**Problem:** Can't login/register
**Solution:** Check API is running and database is connected.

## Development Workflow

### Making Changes to UI

1. Edit files in `task-tracker-ui/src/`
2. Vite will hot-reload changes automatically
3. Check browser console for errors

### Adding New Features

1. Create new components in `src/components/`
2. Add API calls in `src/api/`
3. Update types in `src/types/index.ts`
4. Add routes in `src/App.tsx`

### Building for Production

```powershell
cd task-tracker-ui
npm run build
```

Output will be in `dist/` folder.

## API Endpoints Used

### Authentication
- POST /api/Auth/register
- POST /api/Auth/login
- POST /api/Auth/refresh
- POST /api/Auth/revoke

### Tasks
- GET /api/Tasks (with query parameters)
- GET /api/Tasks/{id}
- POST /api/Tasks
- PUT /api/Tasks/{id}
- DELETE /api/Tasks/{id}

## Features Implemented

✅ JWT Authentication with auto-refresh
✅ User Registration and Login
✅ Protected Routes
✅ Task List with Search and Filters
✅ Create/Edit/Delete Tasks
✅ Status and Priority Management
✅ Tag Support
✅ Due Date Tracking
✅ 24-hour Alert Highlighting
✅ Responsive Design
✅ Toast Notifications
✅ Form Validation
✅ Error Handling
✅ Loading States
✅ Pagination

## Next Steps (Phase 4)

- File Attachments
- Change Password
- User Profile Management
- Email Notifications
- Real-time Updates
- Dark Mode
- Advanced Analytics

## Support

For issues or questions, check:
1. Browser console for errors
2. API logs in terminal
3. Network tab in DevTools
4. PostgreSQL connection status

## Project Structure

```
task-tracker-ui/
├── public/              # Static assets
├── src/
│   ├── api/            # API clients
│   │   ├── axiosConfig.ts
│   │   ├── authApi.ts
│   │   └── taskApi.ts
│   ├── components/     # React components
│   │   ├── Login.tsx
│   │   ├── Register.tsx
│   │   ├── TaskList.tsx
│   │   ├── TaskCard.tsx
│   │   ├── TaskForm.tsx
│   │   └── ProtectedRoute.tsx
│   ├── context/        # React contexts
│   │   └── AuthContext.tsx
│   ├── types/          # TypeScript types
│   │   └── index.ts
│   ├── App.tsx         # Main app component
│   ├── main.tsx        # Entry point
│   └── index.css       # Global styles
├── .env.development    # Dev environment
├── .env.production     # Prod environment
├── Dockerfile          # Docker image
├── nginx.conf          # Nginx config
├── package.json        # Dependencies
├── tsconfig.json       # TypeScript config
├── vite.config.ts      # Vite config
└── tailwind.config.js  # Tailwind config
```
