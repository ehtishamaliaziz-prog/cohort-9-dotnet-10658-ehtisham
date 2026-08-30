import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../services/api";
import Navbar from "../components/Navbar";

function Dashboard() {
  const [summary, setSummary] = useState(null);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const token = localStorage.getItem("token");
  const role = localStorage.getItem("role");
  const isAdmin = role === "Admin";

  useEffect(() => {
    if (!token) {
      navigate("/");
      return;
    }

    const fetchSummary = async () => {
      try {
        const response = await api.get("/tasks/summary", {
          headers: { Authorization: `Bearer ${token}` },
        });
        setSummary(response.data);
      } catch (err) {
        setError("Could not load task summary.");
      }
    };

    fetchSummary();
  }, [navigate, token]);

  return (
    <div>
      <Navbar />
      <div style={{ padding: "1rem" }}>
        <h2>Dashboard</h2>
        <p>{isAdmin ? "Showing counts across all users" : "Showing your task counts"}</p>

        {error && <p style={{ color: "red" }}>{error}</p>}

        {summary && (
          <div style={{ display: "flex", gap: "2rem" }}>
            <div style={{ border: "1px solid #ccc", padding: "1rem", minWidth: "150px" }}>
              <h3>Pending</h3>
              <p style={{ fontSize: "2rem" }}>{summary.pending}</p>
            </div>
            <div style={{ border: "1px solid #ccc", padding: "1rem", minWidth: "150px" }}>
              <h3>In Progress</h3>
              <p style={{ fontSize: "2rem" }}>{summary.inProgress}</p>
            </div>
            <div style={{ border: "1px solid #ccc", padding: "1rem", minWidth: "150px" }}>
              <h3>Completed</h3>
              <p style={{ fontSize: "2rem" }}>{summary.completed}</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default Dashboard;