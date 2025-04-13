import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './Design.module.css';
import { toast } from 'react-toastify';
import { urls } from '../globals';

export default function HomePage() {
  const navigate = useNavigate();
  const [isAdmin, setIsAdmin] = useState(false);

  useEffect(() => {
    verifyIsAdmin();
  }, []);

  const verifyIsAdmin = async () => {
    await fetch(urls.auth.verifyAdmin, {
      method: "GET",
      headers: {
        "Authorization": `Bearer ${sessionStorage.getItem("token")}`
      }
    })
    .then(res => {
      if (res.ok) setIsAdmin(true);
    })
    .catch(err => toast.error(err));
  }

  const logout = () => {
    toast.success("Logout successful");
    sessionStorage.removeItem("token");
    navigate("/login");
  }

  return (
    <div className={styles.buttonGroup}>
      {!sessionStorage.getItem("token") && <button onClick={() => navigate("/login")} className={styles.button}>Login</button>}
      {!sessionStorage.getItem("token") && <button onClick={() => navigate("/register")} className={styles.button}>Register</button>}
      {isAdmin && <button onClick={() => navigate("/admin")} className={styles.button}>Admin Panel</button>}
      {isAdmin && <button onClick={() => navigate("/admin/audit-logs")} className={styles.button}>Audit Logs</button>}
      {sessionStorage.getItem("token") && <button onClick={() => logout()} className={styles.button}>Logout</button>}
    </div>
  )
}