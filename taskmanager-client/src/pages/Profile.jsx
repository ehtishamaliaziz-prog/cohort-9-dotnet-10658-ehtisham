import { useNavigate } from "react-router-dom";
import Navbar from "../components/Navbar";

function Profile() {
  const navigate = useNavigate();

  const fullName = localStorage.getItem("fullName");
  const role = localStorage.getItem("role");
  const token = localStorage.getItem("token");

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    localStorage.removeItem("fullName");
    navigate("/");
  };

  if (!token) {
    navigate("/");
    return null;
  }

  return (
    <div>
      <Navbar />
      <div style={{ padding: "1rem" }}>
        <h2>User Profile</h2>
        <p><strong>Name:</strong> {fullName}</p>
        <p><strong>Role:</strong> {role}</p>
        <button onClick={handleLogout}>Log Out</button>
      </div>
    </div>
  );
}

export default Profile;