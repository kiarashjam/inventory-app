import React, { useEffect, useState } from 'react';
import { Table, Button, Card, Row, Col, Tag, Typography, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const wasteReasonLabels: Record<number, string> = {
  0: 'Spoilage',
  1: 'Overproduction',
  2: 'Customer Return',
  3: 'Preparation',
  4: 'Expired',
  5: 'Damaged',
  6: 'Other',
};

const wasteReasonColors: Record<number, string> = {
  0: 'brown',
  1: 'orange',
  2: 'blue',
  3: 'gold',
  4: 'red',
  5: 'magenta',
  6: 'default',
};

export const WasteRecordList: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [data, setData] = useState<any[]>([]);
  const [total, setTotal] = useState(0);
  const [summary, setSummary] = useState<{ totalRecords?: number; totalCost?: number }>({});
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const loadList = (p: number = page, ps: number = pageSize) => {
    if (orgId === 0) return;
    setLoading(true);
    authorizedAxios
      .get(`/api/inventory/waste/${orgId}`, { params: { page: p, pageSize: ps } })
      .then((res) => {
        const body = res.data?.data ?? res.data;
        const items = Array.isArray(body?.items) ? body.items : (Array.isArray(body) ? body : []);
        setData(items);
      })
      .catch(() => message.error('Failed to load waste records'))
      .finally(() => setLoading(false));
  };

  const loadSummary = () => {
    if (orgId === 0) return;
    authorizedAxios
      .get(`/api/inventory/waste/${orgId}/summary`)
      .then((res) => {
        const d = res.data?.data ?? res.data;
        setSummary(d ?? {});
      })
      .catch(() => {});
  };

  useEffect(() => {
    let mounted = true;
    if (orgId !== 0) {
      loadSummary();
      setLoading(true);
      authorizedAxios
        .get(`/api/inventory/waste/${orgId}`, { params: { page, pageSize } })
        .then((res) => {
          if (mounted) {
            const body = res.data?.data ?? res.data;
            const items = Array.isArray(body?.items) ? body.items : (Array.isArray(body) ? body : []);
            setData(items);
            setTotal(body?.totalCount ?? body?.total ?? items.length);
          }
        })
        .catch(() => { if (mounted) message.error('Failed to load waste records'); })
        .finally(() => { if (mounted) setLoading(false); });
    }
    return () => { mounted = false; };
  }, [orgId, page, pageSize]);

  const formatDate = (d: string | null) => (d ? new Date(d).toLocaleDateString() : '–');

  const columns = [
    { title: 'Date', dataIndex: 'date', key: 'date', width: 110, render: formatDate },
    { title: 'Stock Item Name', dataIndex: 'stockItemName', key: 'stockItemName' },
    { title: 'Quantity', dataIndex: 'quantity', key: 'quantity', width: 100 },
    {
      title: 'Waste Reason',
      dataIndex: 'wasteReason',
      key: 'wasteReason',
      width: 140,
      render: (v: number) => <Tag color={wasteReasonColors[v] ?? 'default'}>{wasteReasonLabels[v] ?? v}</Tag>,
    },
    {
      title: 'Total Cost (CHF)',
      dataIndex: 'totalCost',
      key: 'totalCost',
      width: 120,
      render: (v: number) => (v != null ? `CHF ${Number(v).toFixed(2)}` : '–'),
    },
    { title: 'Recorded By', dataIndex: 'recordedByName', key: 'recordedBy', width: 120 },
    { title: 'Notes', dataIndex: 'notes', key: 'notes', ellipsis: true },
  ];

  if (orgId === 0) return null;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Waste Records</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/waste/new')}>
          Record Waste
        </Button>
      </div>

      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col span={12}>
          <Card size="small">
            <Typography.Text type="secondary">Total Records</Typography.Text>
            <div style={{ fontSize: 24, fontWeight: 600 }}>{summary.totalRecords ?? 0}</div>
          </Card>
        </Col>
        <Col span={12}>
          <Card size="small">
            <Typography.Text type="secondary">Total Cost (CHF)</Typography.Text>
            <div style={{ fontSize: 24, fontWeight: 600 }}>
              CHF {(summary.totalCost ?? 0).toFixed(2)}
            </div>
          </Card>
        </Col>
      </Row>

      <Table
        columns={columns}
        dataSource={data}
        rowKey="id"
        loading={loading}
        pagination={{
          current: page,
          pageSize,
          total,
          showSizeChanger: true,
          showTotal: (t) => `Total ${t} records`,
          onChange: (p, ps) => {
            setPage(p);
            setPageSize(ps ?? pageSize);
          },
        }}
      />
    </div>
  );
};
