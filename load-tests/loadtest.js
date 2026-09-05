import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 100,
  duration: '30s',
};

export default function () {
  const response = http.get('https://localhost:7259/api/products');
  check(response, { 'products endpoint responds': (result) => result.status === 200 });
  sleep(1);
}
