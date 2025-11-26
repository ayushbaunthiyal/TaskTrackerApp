# Task Tracker UI

Modern React TypeScript application for managing tasks with JWT authentication.

## Features

- 🔐 JWT Authentication (Login/Register)
- ✅ Task Management (Create, Read, Update, Delete)
- 🔍 Advanced Search & Filtering
- 📊 Status and Priority Management
- 🏷️ Tag Support
- 📅 Due Date Tracking with Alerts
- 🎨 Modern UI/UX with Tailwind CSS
- 📱 Responsive Design

## Tech Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool
- **React Router** - Navigation
- **Axios** - HTTP client
- **Tailwind CSS** - Styling
- **React Hot Toast** - Notifications
- **date-fns** - Date utilities
- **Lucide React** - Icons

## Getting Started

### Prerequisites

- Node.js 18+
- npm or yarn
- Running Task Tracker API on port 5128

### Installation

```bash
cd task-tracker-ui
npm install
```

### Development

```bash
npm run dev
```

The app will be available at http://localhost:3000

### Build for Production

```bash
npm run build
```

### Environment Variables

Create `.env.development` for local development:
```
VITE_API_BASE_URL=http://localhost:5128/api
```

Create `.env.production` for Docker deployment:
```
VITE_API_BASE_URL=http://tasktracker-api:5128/api
```

## Docker Deployment

Build the Docker image:
```bash
docker build -t task-tracker-ui .
```

Run the container:
```bash
docker run -p 3000:80 task-tracker-ui
```

Or use docker-compose from the root directory:
```bash
docker-compose up
```

## Project Structure

```
src/
├── api/              # API client and configuration
│   ├── axiosConfig.ts
│   ├── authApi.ts
│   └── taskApi.ts
├── components/       # React components
│   ├── Login.tsx
│   ├── Register.tsx
│   ├── TaskList.tsx
│   ├── TaskCard.tsx
│   ├── TaskForm.tsx
│   └── ProtectedRoute.tsx
├── context/          # React context providers
│   └── AuthContext.tsx
├── types/            # TypeScript type definitions
│   └── index.ts
├── App.tsx           # Main application component
├── main.tsx          # Application entry point
└── index.css         # Global styles
```

## Features Details

### Authentication
- JWT-based authentication
- Automatic token refresh on 401
- Protected routes
- Persistent login state

### Task Management
- Create tasks with title, description, status, priority, tags, and due date
- Edit existing tasks
- Delete tasks (with confirmation)
- View all tasks (read access for all users)
- Only owners can modify their tasks

### Search & Filtering
- Real-time search by title/description
- Filter by status (To Do, In Progress, Completed)
- Filter by priority (Low, Medium, High, Critical)
- Sort by various fields
- Pagination support

### UI Features
- Tasks due within 24 hours are highlighted with yellow border
- Color-coded status badges
- Color-coded priority badges
- Responsive grid layout
- Toast notifications for all actions
- Loading states
- Form validation

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## API Integration

The UI communicates with the Task Tracker API:

- **Auth Endpoints:**
  - POST `/api/Auth/login`
  - POST `/api/Auth/register`
  - POST `/api/Auth/refresh`
  - POST `/api/Auth/revoke`

- **Task Endpoints:**
  - GET `/api/Tasks`
  - GET `/api/Tasks/{id}`
  - POST `/api/Tasks`
  - PUT `/api/Tasks/{id}`
  - DELETE `/api/Tasks/{id}`

## License

This project is part of the Task Tracker application suite.
