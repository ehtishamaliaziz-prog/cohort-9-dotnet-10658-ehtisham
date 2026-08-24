import { Outlet } from "react-router-dom";
import Navbar from "./Navbar";
import ProtectedRoute from "./ProtectedRoute";

function AuthenticatedLayout() {
  return (
    <ProtectedRoute>
      <Navbar />
      <Outlet />
    </ProtectedRoute>
  );
}

export default AuthenticatedLayout;