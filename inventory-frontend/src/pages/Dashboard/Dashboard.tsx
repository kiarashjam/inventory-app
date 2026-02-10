import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Statistic, Button, Space, Typography, Table, Tag, Badge } from 'antd';
import { InboxOutlined, WarningOutlined, StopOutlined, ShoppingCartOutlined, PlusOutlined, AuditOutlined, DeleteOutlined, DollarOutlined, AlertOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const movementTypeMap: Record<number, string> = {
  0: 'Received', 1: 'Sold', 2: 'Adjusted', 3: 'Wasted', 4: 'Transferred', 5: 'Returned', 6: 'Count Correction', 7: 'Expired',
};

const movementTypeColors: Record<number, string> = {
  0: 'green', 1: 'blue', 2: 'purple', 3: 'red', 4: 'cyan', 5: 'lime', 6: 'orange', 7: 'magenta',
};

interface DashboardData {
  TotalItems: number;
  LowStock: number;
  OutOfStock: number;
  BelowPar: number;
  TotalInventoryValue: number;
  PendingOrders: number;
  RecentMovements: Array<{
    stockItemName: string;
    movementType: number;
    quantity: number;
    createdAt: string;
  }>;
  WasteThisMonth: number;
  UnreadAlerts: number;
}

export const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const [dashboardData, setDashboardData] = useState<DashboardData>({
    TotalItems: 0,
    LowStock: 0,
    OutOfStock: 0,
    BelowPar: 0,
    TotalInventoryValue: 0,
    PendingOrders: 0,
    RecentMovements: [],
    WasteThisMonth: 0,
    UnreadAlerts: 0,
  });
  const [loading, setLoading] = useState(false);

  const orgId = user?.organizationId || 0;

  useEffect(() => {
    if (orgId === 0) return;
    let mounted = true;
    setLoading(true);
    authorizedAxios.get(`/api/inventory/reports/${orgId}/dashboard`)
      .then(res => {
        if (!mounted) return;
        const data = res.data?.data ?? res.data;
        setDashboardData({
          TotalItems: data?.totalItems ?? data?.TotalItems ?? 0,
          LowStock: data?.lowStock ?? data?.LowStock ?? 0,
          OutOfStock: data?.outOfStock ?? data?.OutOfStock ?? 0,
          BelowPar: data?.belowPar ?? data?.BelowPar ?? 0,
          TotalInventoryValue: data?.totalInventoryValue ?? data?.TotalInventoryValue ?? 0,
          PendingOrders: data?.pendingOrders ?? data?.PendingOrders ?? 0,
          RecentMovements: Array.isArray(data?.recentMovements ?? data?.RecentMovements) ? (data.recentMovements ?? data.RecentMovements).slice(0, 5) : [],
          WasteThisMonth: data?.wasteThisMonth ?? data?.WasteThisMonth ?? 0,
          UnreadAlerts: data?.unreadAlerts ?? data?.UnreadAlerts ?? 0,
        });
      })
      .catch(() => {})
      .finally(() => {
        if (mounted) setLoading(false);
      });
    return () => {
      mounted = false;
    };
  }, [orgId]);

  const movementColumns = [
    { title: 'Stock Item', dataIndex: 'stockItemName', key: 'stockItemName' },
    {
      title: 'Type',
      dataIndex: 'movementType',
      key: 'movementType',
      render: (type: number) => (
        <Tag color={movementTypeColors[type] || 'default'}>
          {movementTypeMap[type] || 'Unknown'}
        </Tag>
      ),
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      render: (qty: number) => (
        <span style={{ color: qty >= 0 ? '#52c41a' : '#ff4d4f' }}>
          {qty >= 0 ? '+' : ''}{qty.toFixed(2)}
        </span>
      ),
    },
    {
      title: 'Date',
      dataIndex: 'createdAt',
      key: 'date',
      render: (date: string) => (date ? new Date(date).toLocaleDateString() : '—'),
    },
  ];

  if (orgId === 0) return null;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Dashboard</Title>
        {dashboardData.UnreadAlerts > 0 && (
          <Badge count={dashboardData.UnreadAlerts} showZero={false}>
            <Button icon={<AlertOutlined />} onClick={() => navigate('/alerts')}>
              Alerts
            </Button>
          </Badge>
        )}
      </div>

      {/* Row 1: 4 stat cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card hoverable onClick={() => navigate('/stock-items')} loading={loading}>
            <Statistic title="Total Items" value={dashboardData.TotalItems} prefix={<InboxOutlined style={{ color: '#1890ff' }} />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card hoverable onClick={() => navigate('/stock-items')} loading={loading}>
            <Statistic title="Low Stock" value={dashboardData.LowStock} prefix={<WarningOutlined style={{ color: '#faad14' }} />} valueStyle={{ color: '#faad14' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card hoverable onClick={() => navigate('/stock-items')} loading={loading}>
            <Statistic title="Out of Stock" value={dashboardData.OutOfStock} prefix={<StopOutlined style={{ color: '#ff4d4f' }} />} valueStyle={{ color: '#ff4d4f' }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card hoverable onClick={() => navigate('/stock-items')} loading={loading}>
            <Statistic title="Below PAR" value={dashboardData.BelowPar} prefix={<WarningOutlined style={{ color: '#faad14' }} />} valueStyle={{ color: '#faad14' }} />
          </Card>
        </Col>
      </Row>

      {/* Row 2: 3 stat cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} sm={12} lg={8}>
          <Card hoverable loading={loading}>
            <Statistic title="Total Inventory Value" value={dashboardData.TotalInventoryValue} prefix={<DollarOutlined style={{ color: '#52c41a' }} />} valueStyle={{ color: '#52c41a' }} suffix="CHF" precision={2} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card hoverable onClick={() => navigate('/purchase-orders')} loading={loading}>
            <Statistic title="Pending Orders" value={dashboardData.PendingOrders} prefix={<ShoppingCartOutlined style={{ color: '#1890ff' }} />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card hoverable onClick={() => navigate('/waste')} loading={loading}>
            <Statistic title="Waste This Month" value={dashboardData.WasteThisMonth} prefix={<DeleteOutlined style={{ color: '#ff4d4f' }} />} valueStyle={{ color: '#ff4d4f' }} suffix="CHF" precision={2} />
          </Card>
        </Col>
      </Row>

      {/* Row 3: Recent Movements and Quick Actions */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={14}>
          <Card title="Recent Movements" loading={loading}>
            <Table
              columns={movementColumns}
              dataSource={dashboardData.RecentMovements}
              rowKey={(record, index) => `${record.stockItemName}-${index}`}
              pagination={false}
              size="small"
            />
          </Card>
        </Col>
        <Col xs={24} lg={10}>
          <Card title="Quick Actions">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Button type="primary" icon={<PlusOutlined />} block onClick={() => navigate('/stock-items/new')}>
                New Stock Item
              </Button>
              <Button icon={<ShoppingCartOutlined />} block onClick={() => navigate('/purchase-orders/new')}>
                New Purchase Order
              </Button>
              <Button icon={<AuditOutlined />} block onClick={() => navigate('/stock-counts/new')}>
                Start Stock Count
              </Button>
              <Button icon={<DeleteOutlined />} block onClick={() => navigate('/waste/new')}>
                Record Waste
              </Button>
            </Space>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
