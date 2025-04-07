import { urls } from "../globals.js"
import { toast } from "react-toastify"
import { useNavigate } from "react-router-dom"

export default function RegisterPage() {
  const navigate = useNavigate();

  const onSubmit = (e) => {
    e.preventDefault();

    if (e.target.password.value !== e.target.confirmPassword.value) {
      toast.error("Passwords do not match.");
      return;
    }

    fetch(urls.auth.register, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        email: e.target.email.value,
        username: e.target.username.value,
        password: e.target.password.value
      })
    })
    .then(res => res.json()).then(data => {
      toast.success(data.message);
      navigate("/login");
    })
    .catch(err => toast.error(err.message));
  }

  return (
    <>
      <form onSubmit={(e) => onSubmit(e)}>
        <div>
          <label>Email:</label>
          <input type="email" name="email" required pattern="[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$" title="Enter a valid email (e.g., user@example.com)" autoComplete="off" spellCheck="false" />
        </div>
        <div>
          <label>Username:</label>
          <input type="test" name="username" required autoComplete="off" spellCheck="false" />
        </div>
        <div>
          <label>Password:</label>
          <input type="password" name="password" required pattern=".{8,}" title="Password must be at least 8 characters long" autoComplete="off" spellCheck="false" />
        </div>
        <div>
          <label>Confirm Password:</label>
          <input type="password" name="confirmPassword" required pattern=".{8,}" title="Password must be at least 8 characters long" autoComplete="off" spellCheck="false" />
        </div>
        <input type="submit" value="Submit" />
      </form>
    </>
  )
}