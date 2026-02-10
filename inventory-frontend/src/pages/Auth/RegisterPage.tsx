import React from 'react';
import { Card, Form, Input, Button, Typography, Space, Alert, Divider } from 'antd';
import { MailOutlined, LockOutlined, UserOutlined, ShopOutlined } from '@ant-design/icons';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, Link } from 'react-router';
import type { RootState, AppDispatch } from '../../redux/store';
import { register, clearError } from '../../redux/slices/authSlice';

const { Title, Text } = Typography;

interface RegisterFormValues {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  organizationName: string;
  currency?: string;
  timezone?: string;
}

export const RegisterPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { status, error, isAuthenticated } = useSelector((state: RootState) => state.auth);
  const hasToken = !!localStorage.getItem('accessToken');

  React.useEffect(() => {
    if (isAuthenticated && hasToken) navigate('/dashboard', { replace: true });
  }, [isAuthenticated, hasToken, navigate]);

  const onFinish = async (values: RegisterFormValues) => {
    const result = await dispatch(register(values));
    if (register.fulfilled.match(result)) {
      navigate('/dashboard');
    }
  };

  React.useEffect(() => { dispatch(clearError()); }, [dispatch]);

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
      <Card style={{ width: 480, borderRadius: 12, boxShadow: '0 8px 32px rgba(0,0,0,0.1)' }}>
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div style={{ textAlign: 'center' }}>
            <Title level={2} style={{ marginBottom: 4 }}>Create Account</Title>
            <Text type="secondary">Set up your restaurant inventory</Text>
          </div>
          {error && <Alert message={error} type="error" showIcon closable onClose={() => dispatch(clearError())} />}
          <Form layout="vertical" onFinish={onFinish} autoComplete="off" size="large">
            <Space style={{ width: '100%' }} size="middle">
              <Form.Item name="firstName" rules={[{ required: true, message: 'Required' }]} style={{ flex: 1 }}>
                <Input prefix={<UserOutlined />} placeholder="First Name" />
              </Form.Item>
              <Form.Item name="lastName" rules={[{ required: true, message: 'Required' }]} style={{ flex: 1 }}>
                <Input prefix={<UserOutlined />} placeholder="Last Name" />
              </Form.Item>
            </Space>
            <Form.Item name="email" rules={[{ required: true, message: 'Please enter your email' }, { type: 'email', message: 'Invalid email' }]}>
              <Input prefix={<MailOutlined />} placeholder="Email" />
            </Form.Item>
            <Form.Item name="password" rules={[{ required: true, message: 'Please enter a password' }, { min: 6, message: 'At least 6 characters' }]}>
              <Input.Password prefix={<LockOutlined />} placeholder="Password" />
            </Form.Item>
            <Form.Item name="organizationName" rules={[{ required: true, message: 'Please enter your restaurant name' }]}>
              <Input prefix={<ShopOutlined />} placeholder="Restaurant Name" />
            </Form.Item>
            <Form.Item>
              <Button type="primary" htmlType="submit" block loading={status === 'loading'}>Create Account</Button>
            </Form.Item>
          </Form>
          <Divider plain><Text type="secondary">or</Text></Divider>
          <div style={{ textAlign: 'center' }}>
            <Text>Already have an account? </Text>
            <Link to="/login">Sign in</Link>
          </div>
        </Space>
      </Card>
    </div>
  );
};
