export interface JwtPayload {
  sub: string; // user ID
  email: string;
  given_name: string; // firstName
  family_name: string; // lastName
  jti: string;
  exp: number;
  iss: string;
  aud: string;
}

export const decodeJwt = (token: string): JwtPayload | null => {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch (error) {
    console.error('Error decoding JWT:', error);
    return null;
  }
};

export const getCurrentUser = (): { email: string; firstName: string; lastName: string } | null => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;

  const payload = decodeJwt(token);
  if (!payload) return null;

  return {
    email: payload.email,
    firstName: payload.given_name,
    lastName: payload.family_name,
  };
};

export const getCurrentUserId = (): string | null => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;

  const payload = decodeJwt(token);
  if (!payload) return null;

  return payload.sub;
};
