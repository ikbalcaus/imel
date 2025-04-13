import { useState, useEffect, useLayoutEffect } from 'react'
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { urls } from '../globals';
import styles from './Design.module.css';

export default function AuditLogs() {
  const navigate = useNavigate();
  const token = sessionStorage.getItem("token");
  const [auditLogs, setAuditLogs] = useState([]);

  useLayoutEffect(() => {
    verifyIsAdmin();
  }, []);

  useEffect(() => {
    fetch(urls.auditLogs, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${sessionStorage.getItem("token")}`
      }
    })
    .then(res => res.json()).then(data => setAuditLogs(data))
    .catch(err => toast.error(err));
  }, []);

  const verifyIsAdmin = async () => {
    await fetch(urls.auth.verifyAdmin, {
      method: "GET",
      headers: {
        "Authorization": `Bearer ${token}`
      }
    })
    .then(res => {
      if (!res.ok) navigate("/");
    })
    .catch(err => toast.error(err));
  }

  return (
    <>
      <pre>{JSON.stringify(auditLogs, null, 2)}</pre>
      <button onClick={() => navigate("/")} className={`${styles.button} ${styles.backButton}`}>Back</button>
    </>
  )
}