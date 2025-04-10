import React, { useState, useEffect } from 'react';
import { DataGrid, Column, Editing, Paging, Pager, FilterRow, Export } from 'devextreme-react/data-grid';
import { Popup } from 'devextreme-react/popup';
import { Button } from 'devextreme-react/button';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { saveAs } from 'file-saver';
import * as XLSX from 'xlsx';
import { urls } from '../globals';
import { toast } from 'react-toastify';
import { useNavigate } from 'react-router-dom';

export default function UsersManagement() {
  const navigate = useNavigate();
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const token = sessionStorage.getItem("token");

  useEffect(() => {
    verifyIsAdmin();
  }, []);

  const verifyIsAdmin = async () => {
    await fetch(urls.auth.verifyAdmin, {
      method: "GET",
      headers: {
        "Authorization": `Bearer ${token}`
      }
    })
    .then(res => {
      if (res.ok) {
        fetchUsers();
        fetchRoles();
      }
      else navigate("/");
    })
    .catch(err => toast.error(err));
  };

  const fetchUsers = async () => {
    await fetch(urls.users, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      }
    })
    .then(res => res.json()).then(data => setUsers(data))
    .catch(err => toast.error(err));
  };

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
  };

  const deleteUser = async (e) => {
    await fetch(urls.users + "/" + e.data.id, {
      method: "DELETE",
      headers: {
        "Authorization": `Bearer ${token}`
      }
    })
    .then(res => res.json()).then(_ => toast.success("Successfully deleted user"))
    .catch(_ => toast.error("An error occurred"));
    fetchUsers();
  };

  const exportToExcel = (e) => {
    const data = users.map(user => ({
      "Email": user.email,
      "Username": user.username,
      "Role": user.role.name,
      "Is active": user.isActive ? "Yes" : "No",
      "Created Date": new Date(user.createdAt).toLocaleDateString(),
      "Last Modified": new Date(user.lastModified).toLocaleDateString()
    }));
    const workbook = XLSX.utils.book_new();
    const worksheet = XLSX.utils.json_to_sheet(data);
    XLSX.utils.book_append_sheet(workbook, worksheet, "Users");
    XLSX.writeFile(workbook, "users.xlsx");
  };

  const exportToPDF = () => {
    const doc = new jsPDF();
    autoTable(doc, {
      head: [["Email", "Username", "Role", "Is active", "Created At", "Last Modified"]],
      body: users.map(user => [
        user.email,
        user.username,
        user.role.name,
        user.isActive ? "Yes" : "No",
        new Date(user.createdAt).toLocaleDateString(),
        new Date(user.lastModified).toLocaleDateString()
      ])
    });
    doc.save("users.pdf");
  };

  const exportToCSV = () => {
    const headers = ["Id,Email,Username,Role,IsActive,CreatedAt,LastModified"];
    const csvContent = [
      ...headers,
      ...users.map(user => 
        `"${user.id}","${user.email}","${user.username}","${user.role.name}","${user.isActive ? 'Yes' : 'No'}","${new Date(user.createdAt).toLocaleDateString()}","${new Date(user.lastModified).toLocaleDateString()}"`
      )
    ].join("\n");
    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    saveAs(blob, "users.csv");
  };

  return (
    <>
      <div>
        <Button text="Export to Excel" icon="xlsxfile" onClick={exportToExcel} />
        <Button text="Export to PDF" icon="pdffile" onClick={exportToPDF} />
        <Button text="Export to CSV" icon="export" onClick={exportToCSV} />
      </div>
      <DataGrid dataSource={users} keyExpr="email" showBorders={true} remoteOperations={true} onRowRemoving={deleteUser} onRowInserting={editUser} onRowUpdating={editUser}>
        <Paging defaultPageSize={10} />
        <Pager showPageSizeSelector={true} allowedPageSizes={[5, 10, 20]} showInfo={true} />
        <FilterRow visible={true} />
        <Editing mode="popup" allowAdding={true} allowUpdating={true} allowDeleting={true} useIcons={true}>
          <Popup title="User Details" showTitle={true} width={700} height={500}/>
        </Editing>
        <Column dataField="email" caption="Email" />
        <Column dataField="username" caption="Username" />
        <Column dataField="password" caption="Password" />
        <Column dataField="roleId" caption="Role" lookup={{dataSource: roles, valueExpr: "id", displayExpr: "name"}} />
        <Column dataField="isActive" caption="Active" dataType="boolean" />
        <Column dataField="createdAt" caption="Created At" dataType="date" format="dd.MM.yyyy" />
        <Column dataField="lastModified" caption="Last Modified" dataType="date" format="dd.MM.yyyy" />
      </DataGrid>
    </>
  );
};