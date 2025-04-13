import { urls } from '../globals.js'
import { toast } from 'react-toastify';
import { useNavigate } from 'react-router-dom';
import styles from './Design.module.css';

export default function LoginPage() {
  const navigate = useNavigate();

  const onSubmit = (e) => {
    e.preventDefault();
    sessionStorage.removeItem("token");

    fetch(urls.auth.login, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        email: e.target.email.value,
        password: e.target.password.value
      })
    })
    .then(res => res.json()).then(data => {
      toast.success(data.message);
      sessionStorage.setItem("token", data.token);
      navigate("/");
    })
    .catch(err => toast.error(err.message));
  }

  return (
    <div style={{ margin: "10px" }}>
      <form onSubmit={(e) => onSubmit(e)} className={styles.form}>
        <div className={styles.inputGroup}>
          <label>Email:</label>
          <input type="email" name="email" required pattern="^[^@\s]+@[^@\s]+\.[^@\s]+$" title="Enter a valid email (e.g., user@example.com)" autoComplete="off" spellCheck="false" />
        </div>
        <div className={styles.inputGroup}>
          <label>Password:</label>
          <input type="password" name="password" required pattern=".{8,}" title="Password must be at least 8 characters long" autoComplete="off" spellCheck="false" />
        </div>
        <input type="submit" value="Submit" className={styles.button} />
      </form>
      <button onClick={() => navigate("/")} className={`${styles.button} ${styles.backButton}`}>Back</button>
    </div>
  )
}