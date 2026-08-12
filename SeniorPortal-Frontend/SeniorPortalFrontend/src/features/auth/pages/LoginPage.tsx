import useRuntimeConfig from '@/hooks/useRuntimeConfig';
import LoginForm from '../components/LoginForm';

export default function LoginPage() {
  const { config } = useRuntimeConfig();

  return (
    <div className='flex flex-col bg-neutralWhite h-screen w-full justify-center items-center px-8'>
      <p className='text-textSecondary text-lg mb-2'>{config.publicName}</p>
      <LoginForm />
    </div>
  );
}
