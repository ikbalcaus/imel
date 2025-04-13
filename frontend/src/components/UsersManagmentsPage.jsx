import { useState, useEffect, useLayoutEffect } from 'react';
import { DataGrid, Column, Editing, FilterRow } from 'devextreme-react/data-grid';
import { Popup } from 'devextreme-react/popup';
import { Button } from 'devextreme-react/button';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { saveAs } from 'file-saver';
import * as XLSX from 'xlsx';
import { urls } from '../globals';
import { toast } from 'react-toastify';
import { useNavigate } from 'react-router-dom';
import styles from './Design.module.css';

export default function UsersManagementPage() {
  const navigate = useNavigate();
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [pages, setPages] = useState([]);
  const [pagination, setPagination] = useState({
    currentPage: 1,
    pageSize: 2, // You can adjust the page size as needed
    totalCount: 0
  });
  const token = sessionStorage.getItem("token");

  useLayoutEffect(() => {
    verifyIsAdmin();
    fetchRoles();
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [pagination.currentPage]);

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

  const fetchUsers = async () => {
    await fetch(urls.users + `?pageNumber=${pagination.currentPage}&pageSize=${pagination.pageSize}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      }
    })
    .then(res => res.json()).then(data => {
      setUsers(data.items);
      setPagination(prev => ({
        ...prev,
        totalCount: data.totalCount
      }));
      setPages(Array.from({ length: Math.ceil(data.totalCount / pagination.pageSize) }, (_, i) => i + 1));
    })
    .catch(err => toast.error(err));
  }

  const fetchRoles = async () => {
    await fetch(urls.roles, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      }
    })
    .then(res => res.json()).then(data => setRoles(data))
    .catch(err => toast.error(err));
  }

  const editUser = async (e) => {
    await fetch(urls.users + "/" + (e.oldData && e.oldData.id ? e.oldData.id : 0), {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      },
      body: JSON.stringify(e.newData || e.data),
    })
    .then(res => res.json()).then(_ => toast.success("Successfully changed data"))
    .catch(_ => toast.error("An error occurred"));
    fetchUsers();
  }

  const deleteUser = async (e) => {
    await fetch(urls.users + "/" + e.data.id, {
      method: "DELETE",
      headers: {
        "Authorization": `Bearer ${token}`
      }
    })
    .then(_ => toast.success("Successfully deleted user"))
    .catch(_ => toast.error("An error occurred"));
    fetchUsers();
  };

  const exportToExcel = (e) => {
    const data = users.map(user => ({
      "Email": user.email,
      "Username": user.username,
      "Role": user.role.name,
      "Is active": user.isActive ? "Yes" : "No",
      "Is deleted": user.isDeleted ? "Yes" : "No"
    }));
    const workbook = XLSX.utils.book_new();
    const worksheet = XLSX.utils.json_to_sheet(data);
    XLSX.utils.book_append_sheet(workbook, worksheet, "Users");
    XLSX.writeFile(workbook, "users.xlsx");
  }

  const exportToPDF = () => {
    const doc = new jsPDF();
    autoTable(doc, {
      head: [["Email", "Username", "Role", "Is active", "Is deleted"]],
      body: users.map(user => [
        user.email,
        user.username,
        user.role.name,
        user.isActive ? "Yes" : "No",
        user.isDeleted ? "Yes" : "No"
      ])
    });
    doc.save("users.pdf");
  }

  const exportToCSV = () => {
    const headers = ["Id,Email,Username,Role,IsActive,IsDeleted"];
    const csvContent = [
      ...headers,
      ...users.map(user => 
        `"${user.id}","${user.email}","${user.username}","${user.role.name}","${user.isActive ? 'Yes' : 'No'}","${user.isDeleted ? 'Yes' : 'No'}"`
      )
    ].join("\n");
    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    saveAs(blob, "users.csv");
  }

  const updatePagination = (page) => {
    setPagination(prev => ({
      ...prev,
      currentPage: page
    }));
    const totalPages = Math.ceil(pagination.totalCount / pagination.pageSize);
    const pages = Array.from({ length: totalPages }, (_, i) => i + 1);
    setPages(pages);
  }

  return (
    <div style={{ margin: "10px" }}>
      <div style={{ display: "flex", gap: "10px" }}>
        <Button text="Export to Excel" icon="xlsxfile" onClick={exportToExcel} />
        <Button text="Export to PDF" icon="pdffile" onClick={exportToPDF} />
        <Button text="Export to CSV" icon="export" onClick={exportToCSV} />
      </div>
      <DataGrid dataSource={users} keyExpr="id" showBorders={true} remoteOperations={true} onRowRemoving={deleteUser} onRowInserting={editUser} onRowUpdating={editUser} onRowClick={(e) => navigate("user-versions/" + e.data.id)}>
      <FilterRow visible={true} />
      <Editing mode="popup" allowAdding={true} allowUpdating={true} allowDeleting={true} useIcons={true}>
        <Popup title="User Details" showTitle={true} width={700} height={500}/>
      </Editing>
      <Column dataField="email" caption="Email" />
      <Column dataField="username" caption="Username" />
      <Column dataField="password" caption="Password" />
      <Column dataField="roleId" caption="Role" lookup={{dataSource: roles, valueExpr: "id", displayExpr: "name"}} />
      <Column dataField="isActive" caption="Active" dataType="boolean" />
      </DataGrid>
      <div className={styles.buttonGroup}>
        {pages.map(page =>
        <button key={page} onClick={() => updatePagination(page)} className={styles.button}>{page}</button>
        )}
      </div>
      <button onClick={() => navigate("/")} className={`${styles.button} ${styles.backButton}`}>Back</button>
    </div>
  );
};