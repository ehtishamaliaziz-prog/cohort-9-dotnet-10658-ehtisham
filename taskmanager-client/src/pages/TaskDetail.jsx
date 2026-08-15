import { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import api from "../services/api";
import Navbar from "../components/Navbar";

function TaskDetail() {
  const [task, setTask] = useState(null);
  const [error, setError] = useState("");
  const { id } = useParams();
  const navigate = useNavigate();

  const token = localStorage.getItem("token");

  useEffect(() => {
    if (!token) {
      navigate("/");
      return;
    }

    const fetchTask = async () => {
      try {
        const response = await api.get(`/tasks/${id}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        setTask(response.data);
      } catch (err) {
        setError("Could not load this task. It may not exist, or you may not have access to it.");
      }
    };

    fetchTask();
  }, [id, navigate, token]);

  return (
    <div>
      <Navbar />
      <div style={{ padding: "1rem" }}>
        <p>
          <Link to="/tasks">&larr; Back to Tasks</Link>
        </p>

        {error && <p style={{ color: "red" }}>{error}</p>}

        {task && (
          <div>
            <h2>{task.title}</h2>
            <p><strong>Description:</strong> {task.description || "-"}</p>
            <p><strong>Status:</strong> {task.status}</p>
            <p><strong>Priority:</strong> {task.priority}</p>
            <p><strong>Category:</strong> {task.category || "-"}</p>
            <p><strong>Due Date:</strong> {task.dueDate ? task.dueDate.split("T")[0] : "-"}</p>
            <p><strong>Created At:</strong> {task.createdAt ? task.createdAt.split("T")[0] : "-"}</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default TaskDetail;