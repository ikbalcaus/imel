import { useState, useEffect, useLayoutEffect } from 'react';
import { DataGrid, Column, Paging, Pager, FilterRow, Export } from 'devextreme-react/data-grid';
import { urls } from '../globals';
import { toast } from 'react-toastify';
import { useNavigate } from 'react-router-dom';
import { useParams } from 'react-router-dom';
import { confirm } from 'devextreme/ui/dialog';
import styles from './Design.module.css';

export default function UserVersionsPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const [userVersions, setUserVersions] = useState([]);
  const [roles, setRoles] = useState([]);
  const [pages, setPages] = useState([]);
  const [pagination, setPagination] = useState({
    currentPage: 1,
    pageSize: 5, // You can adjust the page size as needed
    totalCount: 0
  });
  const token = sessionStorage.getItem("token");

  useLayoutEffect(() => {
    verifyIsAdmin();
    fetchRoles();
  }, []);

  useEffect(() => {
    fetchUserVersions();
  }, [pagination.currentPage]);

  const verifyIsAdmin = async () => {
    await fetch(urls.auth.verifyAdmin, {
      method: "GET",
      headers: {
        "Authorization": `Bearer ${token}`
      }
    })
    .then(res => {
      if (res.ok) {
        fetchUserVersions();
        fetchRoles();
      }
      else navigate("/");
    })
    .catch(err => toast.error(err));
  }

  const fetchUserVersions = async () => {
    await fetch(urls.userVersions + `/${id}?pageNumber=${pagination.currentPage}&pageSize=${pagination.pageSize}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      }
    })
    .then(res => res.json()).then(data => {
      setUserVersions(data.items);
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

  const revertChanges = async (e) => {
    if (!await confirm("Are you sure you want to revert to this record?", "Revert")) return;
    await fetch(urls.userVersions + "/" + e.data.userId, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      },
      body: JSON.stringify({ versionNumber: e.data.versionNumber })
    })
    .then(_ => {
      fetchUserVersions();
      toast.success("Successfully reverted changes");
    })
    .catch(err => toast.error(err));
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
    <div style={{ padding: "10px" }}>
      <DataGrid dataSource={userVersions} keyExpr="id" showBorders={true} onRowClick={revertChanges}>
        <Paging defaultPageSize={10} />
        <Pager showPageSizeSelector={true} allowedPageSizes={[5, 10, 20]} showInfo={true} />
        <FilterRow visible={true} />
        <Column dataField="userData.email" caption="Email" />
        <Column dataField="userData.username" caption="Username" />
        <Column dataField="userData.roleId" caption="Role" lookup={{dataSource: roles, valueExpr: "id", displayExpr: "name"}} />
        <Column dataField="userData.isActive" caption="Active" dataType="boolean" />
        <Column dataField="versionNumber" caption="Version Number" />
        <Column dataField="modifiedAt" caption="Modified At" dataType="date" format="dd.MM.yyyy" />
        <Column dataField="modifiedByUser" caption="Modified By" />
        <Column dataField="action" caption="Action" />
      </DataGrid>
      <div className={styles.buttonGroup}>
        {pages.map(page =>
        <button key={page} onClick={() => updatePagination(page)} className={styles.button}>{page}</button>
        )}
      </div>
      <button onClick={() => navigate(-1)} className={`${styles.button} ${styles.backButton}`}>Back</button>
    </div>
  );
}