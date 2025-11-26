import { useState, FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogIn, CheckSquare, AlertCircle } from 'lucide-react';

export const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!email || !password) return;

    setLoading(true);
    setError('');
    try {
      await login({ email, password });
      navigate('/tasks');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Invalid email or password. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 px-4">
      <div className="max-w-4xl w-full grid md:grid-cols-2 gap-8 items-center">
        {/* Left Side - App Branding */}
        <div className="hidden md:block text-center">
          <div className="inline-block p-4 bg-white rounded-3xl shadow-2xl mb-6">
            <CheckSquare className="w-20 h-20 text-indigo-600" />
          </div>
          <h1 className="text-5xl font-bold text-gray-900 mb-4">Task Tracker</h1>
          <p className="text-xl text-gray-700 mb-6">Your Personal Productivity Hub</p>
          <div className="space-y-4 text-left bg-white/50 backdrop-blur-sm rounded-2xl p-6 shadow-lg">
            <div className="flex items-start gap-3">
              <div className="w-2 h-2 bg-indigo-600 rounded-full mt-2"></div>
              <p className="text-gray-700">Organize and track all your tasks in one place</p>
            </div>
            <div className="flex items-start gap-3">
              <div className="w-2 h-2 bg-indigo-600 rounded-full mt-2"></div>
              <p className="text-gray-700">Set priorities and never miss a deadline</p>
            </div>
            <div className="flex items-start gap-3">
              <div className="w-2 h-2 bg-indigo-600 rounded-full mt-2"></div>
              <p className="text-gray-700">Attach files and collaborate seamlessly</p>
            </div>
            <div className="flex items-start gap-3">
              <div className="w-2 h-2 bg-indigo-600 rounded-full mt-2"></div>
              <p className="text-gray-700">Track your productivity with detailed audit trails</p>
            </div>
          </div>
        </div>

        {/* Right Side - Login Form */}
        <div className="bg-white rounded-2xl shadow-xl p-8">
          <div className="text-center mb-8">
            <div className="inline-block p-3 bg-indigo-100 rounded-full mb-4 md:hidden">
              <CheckSquare className="w-8 h-8 text-indigo-600" />
            </div>
            <h2 className="text-3xl font-bold text-gray-900">Welcome Back</h2>
            <p className="text-gray-600 mt-2">Sign in to continue to Task Tracker</p>
          </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Email Address
            </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-3 rounded-lg border border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
              placeholder="you@example.com"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Password
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-3 rounded-lg border border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
              placeholder="••••••••"
              required
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-indigo-600 text-white py-3 px-4 rounded-lg hover:bg-indigo-700 focus:ring-4 focus:ring-indigo-300 font-medium transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? 'Signing in...' : 'Sign In'}
          </button>

          {error && (
            <div className="mt-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
              <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
              <p className="text-sm text-red-800">{error}</p>
            </div>
          )}
        </form>

          <p className="mt-6 text-center text-gray-600">
            Don't have an account?{' '}
            <Link to="/register" className="text-indigo-600 hover:text-indigo-700 font-medium">
              Sign up
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};
