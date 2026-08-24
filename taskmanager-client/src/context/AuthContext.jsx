import { createContext, useContext, useState } from "react";
import { setToken, clearToken } from "../services/authToken";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [role, setRole] = useState(null);
  const [fullName, setFullName] = useState(null);

  const login = ({ token, role, fullName }) => {
    setToken(token);
    setIsAuthenticated(true);
    setRole(role);
    setFullName(fullName);
  };

  const logout = () => {
    clearToken();
    setIsAuthenticated(false);
    setRole(null);
    setFullName(null);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, role, fullName, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}