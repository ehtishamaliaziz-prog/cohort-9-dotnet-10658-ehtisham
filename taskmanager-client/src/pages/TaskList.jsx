import { useEffect, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import api from "../services/api";
import Navbar from "../components/Navbar";

function TaskList() {
  const [tasks, setTasks] = useState([]);
  const [users, setUsers] = useState([]);
  const [error, setError] = useState("");
  const [searchText, setSearchText] = useState("");
  const [filterStatus, setFilterStatus] = useState("All");
  const [filterPriority, setFilterPriority] = useState("All");
  const navigate = useNavigate();

  const token = localStorage.getItem("token");
  const role = localStorage.getItem("role");
  const isAdmin = role === "Admin";

  const fetchTasks = async () => {
    try {
      const response = await api.get("/tasks", {
        headers: { Authorization: `Bearer ${token}` },
      });
      setTasks(response.data);
    } catch (err) {
      setError("Could not load tasks.");
    }
  };

   const fetchUsers = async () => {
    try {
      const response = await api.get("/auth/users", {
        headers: { Authorization: `Bearer ${token}` },
      });
      setUsers(response.data);
    } catch (err) {
      if (err.response?.status !== 403) {
        setError("Could not load the user list. Reassignment is unavailable.");
      }
    }
  };
  useEffect(() => {
    if (!token) {
      navigate("/");
      return;
    }
    fetchTasks();
    if (isAdmin) {
      fetchUsers();
    }
  }, [navigate, token]);

  const handleDeleteTask = async (taskId) => {
    setError("");
    try {
      await api.delete(`/tasks/${taskId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      fetchTasks();
    } catch (err) {
      setError("Could not delete task.");
    }
  };

  const handleAssignChange = async (task, newUserId) => {
    setError("");
    try {
      await api.put(
        `/tasks/${task.id}`,
        {
          title: task.title,
          description: task.description,
          status: task.status,
          priority: task.priority,
          category: task.category,
          dueDate: task.dueDate,
          userId: Number(newUserId),
        },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      fetchTasks();
    } catch (err) {
      setError("Could not reassign task.");
    }
  };

  const filteredTasks = tasks.filter((task) => {
    const matchesSearch = task.title
      .toLowerCase()
      .includes(searchText.toLowerCase());
    const matchesStatus = filterStatus === "All" || task.status === filterStatus;
    const matchesPriority =
      filterPriority === "All" || task.priority === filterPriority;
    return matchesSearch && matchesStatus && matchesPriority;
  });

  return (
    <div>
      <Navbar />
      <div className="page">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <h2>{isAdmin ? "All Tasks" : "Your Tasks"}</h2>
          <Link to="/tasks/new">
            <button>+ New Task</button>
          </Link>
        </div>

        {error && <p className="error-text">{error}</p>}

        <div className="filters-row">
          <input
            type="text"
            placeholder="Search by title..."
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
          />
          <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}>
            <option value="All">All Statuses</option>
            <option value="Pending">Pending</option>
            <option value="InProgress">In Progress</option>
            <option value="Completed">Completed</option>
          </select>
          <select value={filterPriority} onChange={(e) => setFilterPriority(e.target.value)}>
            <option value="All">All Priorities</option>
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
          </select>
        </div>

        <table>
          <thead>
            <tr>
              <th>Title</th>
              <th>Description</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Category</th>
              <th>Due Date</th>
              {isAdmin && <th>Assigned To</th>}
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {filteredTasks.map((task) => (
              <tr key={task.id}>
                <td>
                  <Link to={`/tasks/${task.id}`}>{task.title}</Link>
                </td>
                <td>{task.description}</td>
                <td>{task.status}</td>
                <td>{task.priority}</td>
                <td>{task.category || "-"}</td>
                <td>{task.dueDate ? task.dueDate.split("T")[0] : "-"}</td>
                {isAdmin && (
                  <td>
                    <select
                      value={task.userId}
                      onChange={(e) => handleAssignChange(task, e.target.value)}
                    >
                      {users.map((u) => (
                        <option key={u.id} value={u.id}>
                          {u.fullName}
                        </option>
                      ))}
                    </select>
                  </td>
                )}
                <td className="actions-cell">
                  <Link to={`/tasks/edit/${task.id}`}>
                    <button className="secondary">Edit</button>
                  </Link>
                  <button className="danger" onClick={() => handleDeleteTask(task.id)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default TaskList;