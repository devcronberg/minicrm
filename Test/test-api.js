document.getElementById('testBtn').addEventListener('click', async () => {
    const resultDiv = document.getElementById('result');
    resultDiv.innerHTML = 'Testing...';

    try {
        const response = await fetch('http://localhost:5000/customers');
        if (response.ok) {
            const data = await response.json();
            resultDiv.innerHTML = `<p>Success! API is accessible.</p><pre>${JSON.stringify(data, null, 2)}</pre>`;
        } else {
            resultDiv.innerHTML = `<p>Error: ${response.status} ${response.statusText}</p>`;
        }
    } catch (error) {
        if (error.name === 'TypeError' && error.message.includes('CORS')) {
            resultDiv.innerHTML = '<p>CORS Error: API is not accessible from browser due to CORS policy.</p>';
        } else {
            resultDiv.innerHTML = `<p>Error: ${error.message}</p>`;
        }
    }
});