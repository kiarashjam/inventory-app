import React, { useEffect, useState } from 'react';
import { Card, Form, Input, InputNumber, Select, Switch, Button, Typography, Descriptions, Space, message, Row, Col } from 'antd';
import { SaveOutlined } from '@ant-design/icons';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

interface OrganizationData {
  name: string;
  address: string;
  city: string;
  postalCode: string;
  country: string;
  phone: string;
  email: string;
  currency: string;
  timezone: string;
  defaultFoodCostTargetPercent: number;
  defaultBeverageCostTargetPercent: number;
  varianceAlertThresholdPercent: number;
  lowStockAlertEmail: boolean;
  autoDeductOnSale: boolean;
  enableHaccp: boolean;
  enableAllergenTracking: boolean;
  enablePrepLists: boolean;
  subscriptionPlan: string;
  createdAt: string;
}

export const SettingsPage: React.FC = () => {
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [orgData, setOrgData] = useState<OrganizationData | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (orgId === 0) return;
    let mounted = true;
    setLoading(true);
    authorizedAxios.get(`/api/inventory/organization/${orgId}`)
      .then(res => {
        if (!mounted) return;
        const data = res.data?.data ?? res.data;
        setOrgData(data);
        form.setFieldsValue({
          name: data?.name || '',
          address: data?.address || '',
          city: data?.city || '',
          postalCode: data?.postalCode || '',
          country: data?.country || '',
          phone: data?.phone || '',
          email: data?.email || '',
          currency: data?.currency || 'CHF',
          timezone: data?.timezone || 'Europe/Zurich',
          defaultFoodCostTargetPercent: data?.defaultFoodCostTargetPercent ?? 30,
          defaultBeverageCostTargetPercent: data?.defaultBeverageCostTargetPercent ?? 25,
          varianceAlertThresholdPercent: data?.varianceAlertThresholdPercent ?? 5,
          lowStockAlertEmail: data?.lowStockAlertEmail ?? false,
          autoDeductOnSale: data?.autoDeductOnSale ?? false,
          enableHaccp: data?.enableHaccp ?? false,
          enableAllergenTracking: data?.enableAllergenTracking ?? false,
          enablePrepLists: data?.enablePrepLists ?? false,
        });
      })
      .catch(() => {
        if (mounted) message.error('Failed to load organization data');
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });
    return () => {
      mounted = false;
    };
  }, [orgId, form]);

  const handleSave = async () => {
    if (orgId === 0) return;
    try {
      const values = await form.validateFields();
      setSaving(true);
      await authorizedAxios.put(`/api/inventory/organization/${orgId}`, values);
      message.success('Settings saved successfully');
      // Reload data
      const res = await authorizedAxios.get(`/api/inventory/organization/${orgId}`);
      const data = res.data?.data ?? res.data;
      setOrgData(data);
    } catch (error: any) {
      if (error?.errorFields) {
        // Form validation error
        return;
      }
      message.error('Failed to save settings');
    } finally {
      setSaving(false);
    }
  };

  const timezones = [
    'Europe/Zurich',
    'Europe/Berlin',
    'Europe/Paris',
    'Europe/London',
    'Europe/Rome',
    'Europe/Madrid',
    'America/New_York',
    'America/Los_Angeles',
    'Asia/Tokyo',
    'Asia/Shanghai',
  ];

  if (orgId === 0) return null;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Settings</Title>
        <Button type="primary" icon={<SaveOutlined />} onClick={handleSave} loading={saving}>
          Save Changes
        </Button>
      </div>

      <Form form={form} layout="vertical" loading={loading}>
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={12}>
            <Card title="Organization Details">
              <Form.Item name="name" label="Name" rules={[{ required: true, message: 'Organization name is required' }]}>
                <Input />
              </Form.Item>
              <Form.Item name="address" label="Address">
                <Input />
              </Form.Item>
              <Form.Item name="city" label="City">
                <Input />
              </Form.Item>
              <Form.Item name="postalCode" label="Postal Code">
                <Input />
              </Form.Item>
              <Form.Item name="country" label="Country">
                <Input />
              </Form.Item>
              <Form.Item name="phone" label="Phone">
                <Input />
              </Form.Item>
              <Form.Item name="email" label="Email" rules={[{ type: 'email', message: 'Invalid email address' }]}>
                <Input />
              </Form.Item>
            </Card>
          </Col>

          <Col xs={24} lg={12}>
            <Card title="Inventory Settings">
              <Form.Item name="currency" label="Currency">
                <Select>
                  <Select.Option value="CHF">CHF</Select.Option>
                  <Select.Option value="EUR">EUR</Select.Option>
                  <Select.Option value="USD">USD</Select.Option>
                  <Select.Option value="GBP">GBP</Select.Option>
                </Select>
              </Form.Item>
              <Form.Item name="timezone" label="Timezone">
                <Select>
                  {timezones.map(tz => (
                    <Select.Option key={tz} value={tz}>{tz}</Select.Option>
                  ))}
                </Select>
              </Form.Item>
              <Form.Item name="defaultFoodCostTargetPercent" label="Food Cost Target %">
                <InputNumber min={0} max={100} style={{ width: '100%' }} addonAfter="%" />
              </Form.Item>
              <Form.Item name="defaultBeverageCostTargetPercent" label="Beverage Cost Target %">
                <InputNumber min={0} max={100} style={{ width: '100%' }} addonAfter="%" />
              </Form.Item>
              <Form.Item name="varianceAlertThresholdPercent" label="Variance Alert Threshold %">
                <InputNumber min={0} max={100} style={{ width: '100%' }} addonAfter="%" />
              </Form.Item>
              <Form.Item name="lowStockAlertEmail" valuePropName="checked">
                <Switch checkedChildren="Low Stock Alert Email" unCheckedChildren="Low Stock Alert Email" />
              </Form.Item>
              <Form.Item name="autoDeductOnSale" valuePropName="checked">
                <Switch checkedChildren="Auto Deduct on Sale" unCheckedChildren="Auto Deduct on Sale" />
              </Form.Item>
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
          <Col xs={24} lg={12}>
            <Card title="Features">
              <Form.Item name="enableHaccp" valuePropName="checked">
                <Switch checkedChildren="Enable HACCP" unCheckedChildren="Enable HACCP" />
              </Form.Item>
              <Form.Item name="enableAllergenTracking" valuePropName="checked">
                <Switch checkedChildren="Enable Allergen Tracking" unCheckedChildren="Enable Allergen Tracking" />
              </Form.Item>
              <Form.Item name="enablePrepLists" valuePropName="checked">
                <Switch checkedChildren="Enable Prep Lists" unCheckedChildren="Enable Prep Lists" />
              </Form.Item>
            </Card>
          </Col>

          <Col xs={24} lg={12}>
            <Card title="Account Info">
              <Descriptions column={1} bordered size="small">
                <Descriptions.Item label="Subscription Plan">
                  {orgData?.subscriptionPlan || '–'}
                </Descriptions.Item>
                <Descriptions.Item label="Created At">
                  {orgData?.createdAt ? new Date(orgData.createdAt).toLocaleString() : '–'}
                </Descriptions.Item>
                <Descriptions.Item label="Current User">
                  {user ? `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.email || '–' : '–'}
                </Descriptions.Item>
                <Descriptions.Item label="User Role">
                  {user?.role || '–'}
                </Descriptions.Item>
                <Descriptions.Item label="User Email">
                  {user?.email || '–'}
                </Descriptions.Item>
              </Descriptions>
            </Card>
          </Col>
        </Row>
      </Form>
    </div>
  );
};
