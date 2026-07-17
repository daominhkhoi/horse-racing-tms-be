const http = require('http');

const data = JSON.stringify({ amount: 100000 });

const options = {
  hostname: '127.0.0.1',
  port: 5032,
  path: '/api/Topup/vnpay/create-url',
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Content-Length': data.length
  }
};

const req = http.request(options, res => {
  let body = '';
  res.on('data', chunk => body += chunk.toString());
  res.on('end', () => console.log(body));
});

req.on('error', error => console.error(error));
req.write(data);
req.end();
