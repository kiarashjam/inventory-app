import React, { useEffect, useState } from 'react';
import { Card, Table, Button, Typography, Spin, Empty } from 'antd';
import { EditOutlined, PlusOutlined } from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const unitLabels: Record<number, string> = {
  0: 'pcs', 1: 'kg', 2: 'g', 3: 'L', 4: 'mL', 5: 'btl', 6: 'box', 7: 'pack', 8: 'doz', 9: 'portion',
};

const foodCostColor = (pct: number | null | undefined): string => {
  if (pct == null) return undefined as any;
  if (pct < 30) return 'green';
  if (pct <= 35) return 'gold';
  return 'red';
};

export const RecipeDetail: React.FC = () => {
  const navigate = useNavigate();
  const { menuItemId } = useParams();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [recipe, setRecipe] = useState<any>(null);
  const [menuItemNameFallback, setMenuItemNameFallback] = useState<string>('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    if (orgId !== 0 && menuItemId) {
      setLoading(true);
      authorizedAxios
        .get(`/api/inventory/recipes/${orgId}/menu-item/${menuItemId}`)
        .then((res) => {
          if (mounted) {
            const data = res.data?.data ?? res.data;
            setRecipe(data);
          }
        })
        .catch(() => { if (mounted) setRecipe(null); })
        .finally(() => { if (mounted) setLoading(false); });
      authorizedAxios.get(`/api/inventory/menu-items/${orgId}/${menuItemId}`).then((res) => {
        if (mounted) {
          const data = res.data?.data ?? res.data;
          if (data?.name) setMenuItemNameFallback(data.name);
        }
      }).catch(() => {});
    }
    return () => { mounted = false; };
  }, [orgId, menuItemId]);

  if (loading && !recipe) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 48 }}>
        <Spin size="large" />
      </div>
    );
  }

  const displayName = menuItemNameFallback || recipe?.menuItemName ?? 'Recipe';
  const sellingPrice = recipe?.sellingPrice ?? 0;
  const totalCost = recipe?.totalCost ?? recipe?.totalRecipeCost ?? 0;
  const foodCostPct = sellingPrice > 0 ? (totalCost / sellingPrice) * 100 : null;
  const ingredients = Array.isArray(recipe?.ingredients) ? recipe.ingredients : [];

  if (!recipe || ingredients.length === 0) {
    return (
      <div>
        <Title level={3}>{displayName}</Title>
        <Card>
          <Empty description="No recipe yet" />
          <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate(`/recipes/${menuItemId}/edit`)} style={{ marginTop: 16 }}>
            Add Ingredients
          </Button>
        </Card>
      </div>
    );
  }

  const columns = [
    { title: 'Stock Item', dataIndex: 'stockItemName', key: 'stockItemName' },
    { title: 'Quantity Required', dataIndex: 'quantityRequired', key: 'quantityRequired', width: 140, render: (v: number) => v?.toFixed(2) ?? '–' },
    { title: 'Unit', dataIndex: 'unitOfMeasurement', key: 'unit', width: 90, render: (u: number) => unitLabels[u] ?? 'pcs' },
    { title: 'Waste %', dataIndex: 'wastePercentage', key: 'waste', width: 90, render: (v: number) => (v ?? 0) + '%' },
    { title: 'Cost per Unit', dataIndex: 'costPerUnit', key: 'costPerUnit', width: 120, render: (v: number) => v != null ? `CHF ${Number(v).toFixed(2)}` : '–' },
    { title: 'SubTotal', dataIndex: 'subTotal', key: 'subTotal', width: 120, render: (v: number) => v != null ? `CHF ${Number(v).toFixed(2)}` : '–' },
  ];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>{displayName}</Title>
        <Button type="primary" icon={<EditOutlined />} onClick={() => navigate(`/recipes/${menuItemId}/edit`)}>Edit Recipe</Button>
      </div>
      <Card style={{ marginBottom: 16 }}>
        <Typography.Paragraph><strong>Selling Price:</strong> CHF {Number(sellingPrice).toFixed(2)}</Typography.Paragraph>
        <Typography.Paragraph><strong>Total Recipe Cost:</strong> CHF {Number(totalCost).toFixed(2)}</Typography.Paragraph>
        <Typography.Paragraph>
          <strong>Food Cost %:</strong>{' '}
          <span style={{ fontSize: 24, fontWeight: 700, color: foodCostColor(foodCostPct) === 'green' ? '#52c41a' : foodCostColor(foodCostPct) === 'gold' ? '#faad14' : '#ff4d4f' }}>
            {foodCostPct != null ? foodCostPct.toFixed(1) : '–'}%
          </span>
        </Typography.Paragraph>
      </Card>
      <Card title="Ingredients">
        <Table
          columns={columns}
          dataSource={ingredients}
          rowKey="stockItemId"
          pagination={false}
          summary={() => (
            <Table.Summary fixed>
              <Table.Summary.Row>
                <Table.Summary.Cell index={0} colSpan={5}><strong>Total</strong></Table.Summary.Cell>
                <Table.Summary.Cell index={1}><strong>CHF {Number(totalCost).toFixed(2)}</strong></Table.Summary.Cell>
              </Table.Summary.Row>
            </Table.Summary>
          )}
        />
      </Card>
    </div>
  );
};
