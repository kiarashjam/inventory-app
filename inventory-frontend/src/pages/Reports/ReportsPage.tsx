import React, { useEffect, useState } from 'react';
import { Tabs, Card, Table, Select, Switch, Space, Button, Typography, Tag, DatePicker, message, Row, Col, Statistic } from 'antd';
import { DownloadOutlined } from '@ant-design/icons';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';
import dayjs, { Dayjs } from 'dayjs';

const { Title } = Typography;
const { RangePicker } = DatePicker;

const unitLabels: Record<number, string> = {
  0: 'pcs', 1: 'kg', 2: 'g', 3: 'L', 4: 'mL', 5: 'btl', 6: 'box', 7: 'pack', 8: 'doz', 9: 'portion',
};

const statusColors: Record<string, string> = {
  InStock: 'green',
  Low: 'orange',
  OutOfStock: 'red',
  BelowPar: 'gold',
};

const movementTypeMap: Record<number, string> = {
  0: 'Received', 1: 'Sold', 2: 'Adjusted', 3: 'Wasted', 4: 'Transferred', 5: 'Returned', 6: 'Count Correction', 7: 'Expired',
};

const movementTypeColors: Record<number, string> = {
  0: 'green', 1: 'blue', 2: 'purple', 3: 'red', 4: 'cyan', 5: 'lime', 6: 'orange', 7: 'magenta',
};

export const ReportsPage: React.FC = () => {
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;

  // Stock Levels state
  const [stockLevels, setStockLevels] = useState<any[]>([]);
  const [stockLevelsLoading, setStockLevelsLoading] = useState(false);
  const [stockLevelsSummary, setStockLevelsSummary] = useState({
    totalValue: 0,
    totalItems: 0,
    lowStockCount: 0,
    outOfStockCount: 0,
  });
  const [categories, setCategories] = useState<any[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | undefined>(undefined);
  const [lowStockOnly, setLowStockOnly] = useState(false);

  // Movement History state
  const [movements, setMovements] = useState<any[]>([]);
  const [movementsLoading, setMovementsLoading] = useState(false);
  const [movementsTotal, setMovementsTotal] = useState(0);
  const [movementsPage, setMovementsPage] = useState(1);
  const [movementsPageSize, setMovementsPageSize] = useState(20);
  const [stockItems, setStockItems] = useState<any[]>([]);
  const [selectedStockItemId, setSelectedStockItemId] = useState<number | undefined>(undefined);
  const [selectedMovementType, setSelectedMovementType] = useState<number | undefined>(undefined);
  const [dateRange, setDateRange] = useState<[Dayjs | null, Dayjs | null] | null>(null);

  // Inventory Valuation state (reuses stock levels data)
  const [valuationData, setValuationData] = useState<any[]>([]);
  const [valuationSummary, setValuationSummary] = useState({
    totalValue: 0,
    avgCostPerItem: 0,
  });

  useEffect(() => {
    if (orgId === 0) return;
    let mounted = true;

    // Load categories
    authorizedAxios.get(`/api/inventory/categories/${orgId}`)
      .then(res => {
        if (!mounted) return;
        const data = res.data?.data ?? res.data;
        setCategories(Array.isArray(data) ? data : []);
      })
      .catch(() => {});

    // Load stock items for movement filter
    authorizedAxios.get(`/api/inventory/stock-items/${orgId}?pageSize=1000`)
      .then(res => {
        if (!mounted) return;
        const data = res.data?.data ?? res.data;
        setStockItems(Array.isArray(data?.items) ? data.items : (Array.isArray(data) ? data : []));
      })
      .catch(() => {});

    return () => {
      mounted = false;
    };
  }, [orgId]);

  // Load stock levels
  useEffect(() => {
    if (orgId === 0) return;
    let mounted = true;
    setStockLevelsLoading(true);
    const params: any = {};
    if (selectedCategoryId) params.categoryId = selectedCategoryId;
    if (lowStockOnly) params.lowStockOnly = true;

    authorizedAxios.get(`/api/inventory/reports/${orgId}/stock-levels`, { params })
      .then(res => {
        if (!mounted) return;
        const data = res.data?.data ?? res.data;
        const items = Array.isArray(data?.items) ? data.items : (Array.isArray(data) ? data : []);
        setStockLevels(items);

        // Calculate summary
        const totalValue = items.reduce((sum: number, item: any) => sum + (item.value ?? item.totalValue ?? 0), 0);
        const lowStock = items.filter((i: any) => (i.status ?? i.stockStatus) === 'Low').length;
        const outOfStock = items.filter((i: any) => (i.status ?? i.stockStatus) === 'OutOfStock').length;
        setStockLevelsSummary({
          totalValue,
          totalItems: items.length,
          lowStockCount: lowStock,
          outOfStockCount: outOfStock,
        });

        // Also set valuation data
        setValuationData(items);
        setValuationSummary({
          totalValue,
          avgCostPerItem: items.length > 0 ? totalValue / items.length : 0,
        });
      })
      .catch(() => {
        if (mounted) message.error('Failed to load stock levels');
      })
      .finally(() => {
        if (mounted) setStockLevelsLoading(false);
      });

    return () => {
      mounted = false;
    };
  }, [orgId, selectedCategoryId, lowStockOnly]);

  // Load movements
  useEffect(() => {
    if (orgId === 0) return;
    let mounted = true;
    setMovementsLoading(true);
    const params: any = {
      page: movementsPage,
      pageSize: movementsPageSize,
    };
    if (selectedStockItemId) params.stockItemId = selectedStockItemId;
    if (selectedMovementType !== undefined) params.movementType = selectedMovementType;
    if (dateRange && dateRange[0] && dateRange[1]) {
      params.startDate = dateRange[0].toISOString();
      params.endDate = dateRange[1].toISOString();
    }

    authorizedAxios.get(`/api/inventory/reports/${orgId}/movements`, { params })
      .then(res => {
        if (!mounted) return;
        const data = res.data?.data ?? res.data;
        const items = Array.isArray(data?.items) ? data.items : (Array.isArray(data) ? data : []);
        setMovements(items);
        setMovementsTotal(typeof data?.totalCount === 'number' ? data.totalCount : items.length);
      })
      .catch(() => {
        if (mounted) message.error('Failed to load movements');
      })
      .finally(() => {
        if (mounted) setMovementsLoading(false);
      });

    return () => {
      mounted = false;
    };
  }, [orgId, movementsPage, movementsPageSize, selectedStockItemId, selectedMovementType, dateRange]);

  const handleExportCSV = async () => {
    if (orgId === 0) return;
    try {
      const params: any = {};
      if (selectedCategoryId) params.categoryId = selectedCategoryId;
      if (lowStockOnly) params.lowStockOnly = true;

      const response = await authorizedAxios.get(`/api/inventory/reports/${orgId}/stock-levels/csv`, {
        params,
        responseType: 'blob',
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `stock-levels-${new Date().toISOString().split('T')[0]}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      message.success('CSV exported successfully');
    } catch {
      message.error('Failed to export CSV');
    }
  };

  const stockLevelsColumns = [
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'SKU', dataIndex: 'sku', key: 'sku', width: 120 },
    { title: 'Category', dataIndex: 'categoryName', key: 'categoryName', width: 120 },
    {
      title: 'Current Qty',
      dataIndex: 'currentQuantity',
      key: 'currentQty',
      width: 120,
      render: (qty: number, record: any) => `${qty.toFixed(2)} ${unitLabels[record.baseUnitOfMeasurement] || 'pcs'}`,
    },
    {
      title: 'Min Threshold',
      dataIndex: 'minimumThreshold',
      key: 'minThreshold',
      width: 120,
      render: (val: number, record: any) => val ? `${val.toFixed(2)} ${unitLabels[record.baseUnitOfMeasurement] || 'pcs'}` : '—',
    },
    {
      title: 'PAR Level',
      dataIndex: 'parLevel',
      key: 'parLevel',
      width: 120,
      render: (val: number, record: any) => val ? `${val.toFixed(2)} ${unitLabels[record.baseUnitOfMeasurement] || 'pcs'}` : '—',
    },
    {
      title: 'Cost Price',
      dataIndex: 'costPrice',
      key: 'costPrice',
      width: 120,
      render: (val: number) => val ? `CHF ${val.toFixed(2)}` : '—',
    },
    {
      title: 'Avg Cost',
      dataIndex: 'averageCostPrice',
      key: 'avgCost',
      width: 120,
      render: (val: number) => val ? `CHF ${val.toFixed(2)}` : '—',
    },
    {
      title: 'Value (CHF)',
      dataIndex: 'value',
      key: 'value',
      width: 120,
      render: (val: number) => val ? `CHF ${val.toFixed(2)}` : 'CHF 0.00',
    },
    { title: 'Supplier', dataIndex: 'supplierName', key: 'supplierName', width: 150 },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (status: string) => <Tag color={statusColors[status] || 'default'}>{status}</Tag>,
    },
  ];

  const movementsColumns = [
    { title: 'Stock Item Name', dataIndex: 'stockItemName', key: 'stockItemName' },
    {
      title: 'Type',
      dataIndex: 'movementType',
      key: 'movementType',
      width: 150,
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
      width: 120,
      render: (qty: number) => (
        <span style={{ color: qty >= 0 ? '#52c41a' : '#ff4d4f' }}>
          {qty >= 0 ? '+' : ''}{qty.toFixed(2)}
        </span>
      ),
    },
    {
      title: 'Previous Qty',
      dataIndex: 'previousQuantity',
      key: 'previousQty',
      width: 120,
      render: (qty: number) => qty !== undefined ? qty.toFixed(2) : '—',
    },
    {
      title: 'New Qty',
      dataIndex: 'newQuantity',
      key: 'newQty',
      width: 120,
      render: (qty: number) => qty !== undefined ? qty.toFixed(2) : '—',
    },
    {
      title: 'Cost',
      dataIndex: 'totalCost',
      key: 'cost',
      width: 120,
      render: (cost: number) => cost ? `CHF ${cost.toFixed(2)}` : '—',
    },
    { title: 'Reason', dataIndex: 'reason', key: 'reason', width: 200 },
    {
      title: 'Date',
      dataIndex: 'createdAt',
      key: 'date',
      width: 180,
      render: (date: string) => (date ? new Date(date).toLocaleString() : '—'),
    },
    { title: 'Created By', dataIndex: 'createdBy', key: 'createdBy', width: 150 },
  ];

  const valuationColumns = [
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'Category', dataIndex: 'categoryName', key: 'categoryName', width: 150 },
    {
      title: 'Current Qty',
      dataIndex: 'currentQuantity',
      key: 'currentQty',
      width: 120,
      render: (qty: number, record: any) => `${qty.toFixed(2)} ${unitLabels[record.baseUnitOfMeasurement] || 'pcs'}`,
    },
    {
      title: 'Avg Cost',
      dataIndex: 'averageCostPrice',
      key: 'avgCost',
      width: 120,
      render: (val: number) => val ? `CHF ${val.toFixed(2)}` : '—',
    },
    {
      title: 'Total Value',
      dataIndex: 'value',
      key: 'totalValue',
      width: 120,
      render: (val: number) => val ? `CHF ${val.toFixed(2)}` : 'CHF 0.00',
    },
    {
      title: '% of Total Value',
      key: 'percent',
      width: 150,
      render: (_: any, record: any) => {
        const percent = valuationSummary.totalValue > 0
          ? ((record.value || 0) / valuationSummary.totalValue * 100).toFixed(2)
          : '0.00';
        return `${percent}%`;
      },
    },
  ];

  // Get top 5 highest value items
  const topValueItems = [...valuationData]
    .sort((a, b) => (b.value ?? b.totalValue ?? 0) - (a.value ?? a.totalValue ?? 0))
    .slice(0, 5);

  if (orgId === 0) return null;

  return (
    <div>
      <Title level={3}>Reports</Title>
      <Tabs
        items={[
          {
            key: 'stock-levels',
            label: 'Stock Levels',
            children: (
              <div>
                <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
                  <Col xs={24} sm={12} lg={6}>
                    <Card>
                      <Statistic title="Total Value" value={stockLevelsSummary.totalValue} suffix="CHF" precision={2} />
                    </Card>
                  </Col>
                  <Col xs={24} sm={12} lg={6}>
                    <Card>
                      <Statistic title="Total Items" value={stockLevelsSummary.totalItems} />
                    </Card>
                  </Col>
                  <Col xs={24} sm={12} lg={6}>
                    <Card>
                      <Statistic title="Low Stock Count" value={stockLevelsSummary.lowStockCount} valueStyle={{ color: '#faad14' }} />
                    </Card>
                  </Col>
                  <Col xs={24} sm={12} lg={6}>
                    <Card>
                      <Statistic title="Out of Stock Count" value={stockLevelsSummary.outOfStockCount} valueStyle={{ color: '#ff4d4f' }} />
                    </Card>
                  </Col>
                </Row>
                <Card>
                  <Space style={{ marginBottom: 16 }}>
                    <Select
                      placeholder="Filter by Category"
                      allowClear
                      style={{ width: 200 }}
                      value={selectedCategoryId}
                      onChange={setSelectedCategoryId}
                    >
                      {categories.map(cat => (
                        <Select.Option key={cat.id} value={cat.id}>{cat.name}</Select.Option>
                      ))}
                    </Select>
                    <Switch
                      checked={lowStockOnly}
                      onChange={setLowStockOnly}
                      checkedChildren="Low Stock Only"
                      unCheckedChildren="All Items"
                    />
                    <Button icon={<DownloadOutlined />} onClick={handleExportCSV}>
                      Export CSV
                    </Button>
                  </Space>
                  <Table
                    columns={stockLevelsColumns}
                    dataSource={stockLevels}
                    rowKey="id"
                    loading={stockLevelsLoading}
                    pagination={{ pageSize: 20, showSizeChanger: true }}
                  />
                </Card>
              </div>
            ),
          },
          {
            key: 'movements',
            label: 'Movement History',
            children: (
              <div>
                <Card>
                  <Space style={{ marginBottom: 16 }} wrap>
                    <Select
                      placeholder="Filter by Stock Item"
                      allowClear
                      style={{ width: 200 }}
                      value={selectedStockItemId}
                      onChange={setSelectedStockItemId}
                    >
                      {stockItems.map(item => (
                        <Select.Option key={item.id} value={item.id}>{item.name}</Select.Option>
                      ))}
                    </Select>
                    <Select
                      placeholder="Filter by Movement Type"
                      allowClear
                      style={{ width: 200 }}
                      value={selectedMovementType}
                      onChange={setSelectedMovementType}
                    >
                      {Object.entries(movementTypeMap).map(([key, label]) => (
                        <Select.Option key={key} value={parseInt(key)}>{label}</Select.Option>
                      ))}
                    </Select>
                    <RangePicker
                      value={dateRange}
                      onChange={(dates) => setDateRange(dates as [Dayjs | null, Dayjs | null] | null)}
                    />
                  </Space>
                  <Table
                    columns={movementsColumns}
                    dataSource={movements}
                    rowKey="id"
                    loading={movementsLoading}
                    pagination={{
                      current: movementsPage,
                      pageSize: movementsPageSize,
                      total: movementsTotal,
                      showSizeChanger: true,
                      showTotal: (t) => `Total ${t} movements`,
                      onChange: (p, ps) => {
                        setMovementsPage(p);
                        setMovementsPageSize(ps ?? 20);
                      },
                    }}
                  />
                </Card>
              </div>
            ),
          },
          {
            key: 'valuation',
            label: 'Inventory Valuation',
            children: (
              <div>
                <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
                  <Col xs={24} sm={12} lg={8}>
                    <Card>
                      <Statistic title="Total Value" value={valuationSummary.totalValue} suffix="CHF" precision={2} />
                    </Card>
                  </Col>
                  <Col xs={24} sm={12} lg={8}>
                    <Card>
                      <Statistic title="Avg Cost per Item" value={valuationSummary.avgCostPerItem} suffix="CHF" precision={2} />
                    </Card>
                  </Col>
                  <Col xs={24} sm={12} lg={8}>
                    <Card title="Highest Value Items">
                      <Table
                        columns={[
                          { title: 'Name', dataIndex: 'name', key: 'name' },
                          {
                            title: 'Value',
                            dataIndex: 'value',
                            key: 'value',
                            render: (val: number) => `CHF ${(val || 0).toFixed(2)}`,
                          },
                        ]}
                        dataSource={topValueItems}
                        rowKey="id"
                        pagination={false}
                        size="small"
                      />
                    </Card>
                  </Col>
                </Row>
                <Card>
                  <Table
                    columns={valuationColumns}
                    dataSource={valuationData}
                    rowKey="id"
                    loading={stockLevelsLoading}
                    pagination={{ pageSize: 20, showSizeChanger: true }}
                  />
                </Card>
              </div>
            ),
          },
        ]}
      />
    </div>
  );
};
