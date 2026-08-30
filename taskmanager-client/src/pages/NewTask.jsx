import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import api from "../services/api";
import Navbar from "../components/Navbar";

function NewTask() {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState("Pending");
  const [priority, setPriority] = useState("Medium");
  const [category, setCategory] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const token = localStorage.getItem("token");

  useEffect(() => {
    if (!token) {
      navigate("/");
      return;
    }

    if (isEditing) {
      const fetchTask = async () => {
        try {
          const response = await api.get(`/tasks/${id}`, {
            headers: { Authorization: `Bearer ${token}` },
          });
          const task = response.data;
          setTitle(task.title);
          setDescription(task.description || "");
          setStatus(task.status);
          setPriority(task.priority);
          setCategory(task.category || "");
          setDueDate(task.dueDate ? task.dueDate.split("T")[0] : "");
        } catch (err) {
          setError("Could not load this task.");
        }
      };
      fetchTask();
    }
  }, [id, isEditing, navigate, token]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    const payload = {
      title,
      description,
      status,
      priority,
      category,
      dueDate: dueDate ? dueDate : null,
    };

    try {
      if (isEditing) {
        await api.put(`/tasks/${id}`, payload, {
          headers: { Authorization: `Bearer ${token}` },
        });
      } else {
        await api.post("/tasks", payload, {
          headers: { Authorization: `Bearer ${token}` },
        });
      }
      navigate("/tasks");
    } catch (err) {
      setError(isEditing ? "Could not update task." : "Could not create task.");
    }
  };

  return (
    <div>
      <Navbar />
      <div className="page">
        <h2>{isEditing ? "Edit Task" : "New Task"}</h2>

        {error && <p className="error-text">{error}</p>}

        <form className="task-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="task-title">Title</label>
            <input
              id="task-title"
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="task-description">Description</label>
            <input
              id="task-description"
              type="text"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>
          <div className="form-group">
            <label htmlFor="task-status">Status</label>
            <select id="task-status" value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Pending">Pending</option>
              <option value="InProgress">In Progress</option>
              <option value="Completed">Completed</option>
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="task-priority">Priority</label>
            <select id="task-priority" value={priority} onChange={(e) => setPriority(e.target.value)}>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="task-category">Category</label>
            <input
              id="task-category"
              type="text"
              value={category}
              onChange={(e) => setCategory(e.target.value)}
            />
          </div>
          <div className="form-group">
            <label htmlFor="task-duedate">Due Date</label>
            <input
              id="task-duedate"
              type="date"
              value={dueDate}
              onChange={(e) => setDueDate(e.target.value)}
            />
          </div>
          <div className="form-actions">
            <button type="submit">{isEditing ? "Update Task" : "Create Task"}</button>
            <button type="button" className="secondary" onClick={() => navigate("/tasks")}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default NewTask;