export enum TaskStatus {
  Pending = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4,
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface Task {
  id: string;
  userId: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  tags: string[];
  dueDate: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface TaskFormData {
  Title: string;
  Description: string;
  Status: TaskStatus;
  Priority: TaskPriority;
  Tags: string[];
  DueDate: string | null;
}

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface TaskFilters {
  searchTerm?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  sortBy?: string;
  sortDescending?: boolean;
  pageNumber: number;
  pageSize: number;
}
