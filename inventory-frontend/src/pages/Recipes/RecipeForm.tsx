import React, { useEffect, useState } from 'react';
import { Form, Input, InputNumber, Select, Button, Card, Typography, Space, message, Spin } from 'antd';
import { PlusOutlined, MinusCircleOutlined } from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const unitLabels: Record<number, string> = {
  0: 'pcs', 1: 'kg', 2: 'g', 3: 'L', 4: 'mL', 5: 'btl', 6: 'box', 7: 'pack', 8: 'doz', 9: 'portion',
};

const unitOptions = [
  { value: 0, label: 'pcs' }, { value: 1, label: 'kg' }, { value: 2, label: 'g' },
  { value: 3, label: 'L' }, { value: 4, label: 'mL' }, { value: 5, label: 'btl' },
  { value: 6, label: 'box' }, { value: 7, label: 'pack' }, { value: 8, label: 'doz' }, { value: 9, label: 'portion' },
];

const foodCostColor = (pct: number | null | undefined): string => {
  if (pct == null) return '#000';
  if (pct < 30) return '#52c41a';
  if (pct <= 35) return '#faad14';
  return '#ff4d4f';
};

export const RecipeForm: React.FC = () => {
  const navigate = useNavigate();
  const { menuItemId } = useParams();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [stockItems, setStockItems] = useState<any[]>([]);
  const [sellingPrice, setSellingPrice] = useState<number>(0);

  useEffect(() => {
    let mounted = true;
    if (orgId === 0 || !menuItemId) return;
    setFetching(true);
    Promise.all([
      authorizedAxios.get(`/api/inventory/recipes/${orgId}/menu-item/${menuItemId}`),
      authorizedAxios.get(`/api/inventory/stock-items/${orgId}?pageSize=500`),
      authorizedAxios.get(`/api/inventory/menu-items/${orgId}/${menuItemId}`).catch(() => ({ data: {} })),
    ])
      .then(([recipeRes, stockRes, menuRes]) => {
        if (!mounted) return;
        const recipe = recipeRes.data?.data ?? recipeRes.data;
        const menuData = menuRes.data?.data ?? menuRes.data;
        setSellingPrice(menuData?.sellingPrice ?? recipe?.sellingPrice ?? 0);
        const list = stockRes.data?.data?.items ?? stockRes.data?.items ?? (Array.isArray(stockRes.data?.data) ? stockRes.data.data : (Array.isArray(stockRes.data) ? stockRes.data : []));
        setStockItems(Array.isArray(list) ? list : []);
        const ingredients = Array.isArray(recipe?.ingredients) ? recipe.ingredients : [];
        if (ingredients.length > 0) {
          form.setFieldsValue({
            ingredients: ingredients.map((i: any) => ({
              stockItemId: i.stockItemId,
              quantityRequired: i.quantityRequired,
              unitOfMeasurement: i.unitOfMeasurement ?? 0,
              wastePercentage: i.wastePercentage ?? 0,
              notes: i.notes,
            })),
          });
        } else {
          form.setFieldsValue({ ingredients: [] });
        }
      })
      .catch(() => { if (mounted) message.error('Failed to load recipe'); })
      .finally(() => { if (mounted) setFetching(false); });
    return () => { mounted = false; };
  }, [orgId, menuItemId, form]);

  const onFinish = async (values: any) => {
    if (orgId === 0 || !menuItemId) return;
    const ingredients = (values.ingredients ?? []).map((row: any) => ({
      stockItemId: row.stockItemId,
      quantityRequired: row.quantityRequired,
      unitOfMeasurement: row.unitOfMeasurement ?? 0,
      wastePercentage: row.wastePercentage ?? 0,
      notes: row.notes,
    }));
    setLoading(true);
    try {
      await authorizedAxios.post(`/api/inventory/recipes/${orgId}/menu-item/${menuItemId}`, ingredients);
      message.success('Recipe saved');
      navigate(`/recipes/${menuItemId}`);
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to save');
    } finally {
      setLoading(false);
    }
  };

  const getCostForItem = (stockItemId: number) => {
    const item = stockItems.find((s: any) => s.id === stockItemId);
    return item?.costPrice ?? item?.averageCostPrice ?? 0;
  };

  if (fetching) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 48 }}>
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div>
      <Title level={3}>Edit Recipe</Title>
      <Card>
        <Form form={form} onFinish={onFinish} initialValues={{ ingredients: [] }}>
          <Form.List name="ingredients">
            {(fields, { add, remove }) => (
              <>
                {fields.map(({ key, name, ...restField }) => (
                  <Space key={key} style={{ display: 'flex', marginBottom: 8 }} align="baseline">
                    <Form.Item {...restField} name={[name, 'stockItemId']} rules={[{ required: true, message: 'Required' }]} style={{ width: 220 }}>
                      <Select
                        placeholder="Stock Item"
                        showSearch
                        optionFilterProp="label"
                        options={stockItems.map((s: any) => ({ value: s.id, label: s.name }))}
                      />
                    </Form.Item>
                    <Form.Item {...restField} name={[name, 'quantityRequired']} rules={[{ required: true, message: 'Required' }]} style={{ width: 120 }}>
                      <InputNumber min={0.001} step={0.01} placeholder="Qty" style={{ width: '100%' }} />
                    </Form.Item>
                    <Form.Item {...restField} name={[name, 'unitOfMeasurement']} initialValue={0} style={{ width: 100 }}>
                      <Select options={unitOptions} placeholder="Unit" />
                    </Form.Item>
                    <Form.Item {...restField} name={[name, 'wastePercentage']} initialValue={0} style={{ width: 90 }}>
                      <InputNumber min={0} max={100} step={1} placeholder="Waste %" style={{ width: '100%' }} />
                    </Form.Item>
                    <Form.Item {...restField} name={[name, 'notes']} style={{ width: 140 }}>
                      <Input placeholder="Notes" />
                    </Form.Item>
                    <MinusCircleOutlined onClick={() => remove(name)} style={{ color: '#ff4d4f' }} />
                  </Space>
                ))}
                <Form.Item>
                  <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />}>Add Ingredient</Button>
                </Form.Item>
              </>
            )}
          </Form.List>
          <Form.Item shouldUpdate={(prev, curr) => prev.ingredients !== curr.ingredients}>
            {({ getFieldValue }) => {
              const ingredients = getFieldValue('ingredients') ?? [];
              let total = 0;
              ingredients.forEach((row: any, idx: number) => {
                const qty = row?.quantityRequired ?? 0;
                const waste = (row?.wastePercentage ?? 0) / 100;
                const cost = getCostForItem(row?.stockItemId);
                total += qty * (1 + waste) * cost;
              });
              const pct = sellingPrice > 0 ? (total / sellingPrice) * 100 : null;
              return (
                <Typography.Paragraph>
                  <strong>Running total:</strong> CHF {total.toFixed(2)}
                  {pct != null && (
                    <>
                      {' '}
                      <strong style={{ color: foodCostColor(pct) }}>Food cost: {pct.toFixed(1)}%</strong>
                    </>
                  )}
                </Typography.Paragraph>
              );
            }}
          </Form.Item>
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={loading}>Save</Button>
              <Button onClick={() => navigate(`/recipes/${menuItemId}`)}>Cancel</Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};
