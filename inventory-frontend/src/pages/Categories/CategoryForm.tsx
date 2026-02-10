import React, { useEffect, useState } from 'react';
import { Form, Input, InputNumber, Select, Switch, Button, Card, Typography, Space, message } from 'antd';
import { useNavigate, useParams } from 'react-router';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState, AppDispatch } from '../../redux/store';
import { fetchCategories } from '../../redux/slices/inventorySlice';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

function flattenCategories(cats: any[], out: any[] = []): any[] {
  if (!Array.isArray(cats)) return out;
  for (const c of cats) {
    out.push(c);
    if (Array.isArray(c.subCategories) && c.subCategories.length) flattenCategories(c.subCategories, out);
  }
  return out;
}

export const CategoryForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const dispatch = useDispatch<AppDispatch>();
  const isEdit = !!id;
  const { user } = useSelector((state: RootState) => state.auth);
  const { categories } = useSelector((state: RootState) => state.inventory);
  const orgId = user?.organizationId || 0;
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (orgId !== 0) dispatch(fetchCategories(orgId));
  }, [dispatch, orgId]);

  useEffect(() => {
    if (orgId === 0 || !isEdit || !id) return;
    let mounted = true;
    authorizedAxios
      .get(`/api/inventory/categories/${orgId}/${id}`)
      .then((r) => {
        if (mounted) {
          const data = r.data?.data ?? r.data;
          if (data) form.setFieldsValue(data);
        }
      })
      .catch(() => {
        if (mounted) message.error('Failed to load category');
      });
    return () => {
      mounted = false;
    };
  }, [orgId, id, isEdit, form]);

  const onFinish = async (values: any) => {
    if (orgId === 0) return;
    setLoading(true);
    try {
      if (isEdit) {
        await authorizedAxios.put(`/api/inventory/categories/${orgId}/${id}`, values);
        message.success('Category updated');
      } else {
        await authorizedAxios.post(`/api/inventory/categories/${orgId}`, values);
        message.success('Category created');
      }
      navigate('/categories');
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to save');
    } finally {
      setLoading(false);
    }
  };

  const flat = flattenCategories(categories);
  const parentOptions = flat
    .filter((c: any) => !isEdit || String(c.id) !== id)
    .map((c: any) => ({ value: c.id, label: c.name }));

  if (orgId === 0) return null;

  return (
    <div>
      <Title level={3}>{isEdit ? 'Edit Category' : 'New Category'}</Title>
      <Card>
        <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ displayOrder: 0, isActive: true }}>
          <Form.Item name="name" label="Name" rules={[{ required: true, message: 'Name is required' }]}>
            <Input placeholder="Category name" />
          </Form.Item>
          <Form.Item name="displayOrder" label="Display Order">
            <InputNumber style={{ width: '100%' }} min={0} />
          </Form.Item>
          <Form.Item name="parentCategoryId" label="Parent Category">
            <Select allowClear placeholder="Select parent (optional)" options={parentOptions} />
          </Form.Item>
          <Form.Item name="isActive" label="Active" valuePropName="checked">
            <Switch />
          </Form.Item>
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={loading}>Save</Button>
              <Button onClick={() => navigate('/categories')}>Cancel</Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};
