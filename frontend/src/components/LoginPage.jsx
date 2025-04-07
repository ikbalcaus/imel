import { urls } from "../globals.js"
import { toast } from "react-toastify";

export default function LoginPage() {
  const onSubmit = (e) => {
    e.preventDefault();

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
    })
    .catch(err => toast.error(err.message));
  }

  return (
    <>
      <form onSubmit={(e) => onSubmit(e)}>
        <div>
          <label>Email:</label>
          <input type="email" name="email" required pattern="^[^@\s]+@[^@\s]+\.[^@\s]+$" title="Enter a valid email (e.g., user@example.com)" autoComplete="off" spellCheck="false" />
        </div>
        <div>
          <label>Password:</label>
          <input type="password" name="password" required pattern=".{8,}" title="Password must be at least 8 characters long" autoComplete="off" spellCheck="false" />
        </div>
        <input type="submit" value="Submit" />
      </form>
    </>
  )
}