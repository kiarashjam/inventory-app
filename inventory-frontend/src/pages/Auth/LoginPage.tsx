import React from 'react';
import { Card, Form, Input, Button, Typography, Space, Alert, Divider } from 'antd';
import { MailOutlined, LockOutlined } from '@ant-design/icons';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, Link } from 'react-router';
import type { RootState, AppDispatch } from '../../redux/store';
import { login, clearError } from '../../redux/slices/authSlice';

const { Title, Text } = Typography;

export const LoginPage: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { status, error, isAuthenticated } = useSelector((state: RootState) => state.auth);
  const hasToken = !!localStorage.getItem('accessToken');

  React.useEffect(() => {
    if (isAuthenticated && hasToken) navigate('/dashboard', { replace: true });
  }, [isAuthenticated, hasToken, navigate]);

  const onFinish = async (values: { email: string; password: string }) => {
    const result = await dispatch(login(values));
    if (login.fulfilled.match(result)) {
      navigate('/dashboard');
    }
  };

  React.useEffect(() => { dispatch(clearError()); }, [dispatch]);

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
      <Card style={{ width: 420, borderRadius: 12, boxShadow: '0 8px 32px rgba(0,0,0,0.1)' }}>
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div style={{ textAlign: 'center' }}>
            <Title level={2} style={{ marginBottom: 4 }}>Inventory Pro</Title>
            <Text type="secondary">Sign in to your account</Text>
          </div>
          {error && <Alert message={error} type="error" showIcon closable onClose={() => dispatch(clearError())} />}
          <Form layout="vertical" onFinish={onFinish} autoComplete="off" size="large">
            <Form.Item name="email" rules={[{ required: true, message: 'Please enter your email' }, { type: 'email', message: 'Invalid email' }]}>
              <Input prefix={<MailOutlined />} placeholder="Email" />
            </Form.Item>
            <Form.Item name="password" rules={[{ required: true, message: 'Please enter your password' }]}>
              <Input.Password prefix={<LockOutlined />} placeholder="Password" />
            </Form.Item>
            <Form.Item>
              <Button type="primary" htmlType="submit" block loading={status === 'loading'}>Sign In</Button>
            </Form.Item>
          </Form>
          <Divider plain><Text type="secondary">or</Text></Divider>
          <div style={{ textAlign: 'center' }}>
            <Text>Don't have an account? </Text>
            <Link to="/register">Create one</Link>
          </div>
        </Space>
      </Card>
    </div>
  );
};
