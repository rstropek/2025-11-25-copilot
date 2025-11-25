import './style.css'

// Get API base URL from environment or default
const API_BASE_URL = import.meta.env.VITE_API_URL || '';

// Database endpoints configuration - matching WebApi/database-endpoints.json
const endpoints = [
  {
    route: "/customers/filtered",
    method: "GET",
    query: "SELECT * FROM Customers WHERE (Country = $country OR $country IS NULL) AND (Email LIKE $emailPattern OR $emailPattern IS NULL) AND (Revenue >= $minRevenue OR $minRevenue IS NULL)",
    parameters: {
      country: { type: "string", optional: true },
      emailPattern: { type: "string", optional: true },
      minRevenue: { type: "decimal", optional: true }
    },
    returnType: "Array"
  },
  {
    route: "/customers/all",
    method: "GET",
    query: "SELECT * FROM Customers",
    returnType: "Array"
  },
  {
    route: "/customers/by-id",
    method: "GET",
    query: "SELECT * FROM Customers WHERE Id = $id",
    parameters: {
      id: { type: "int", optional: false }
    },
    returnType: "Single"
  }
];

let selectedEndpoint = null;

function renderEndpoints() {
  const container = document.getElementById('endpoints-list');
  
  container.innerHTML = endpoints.map((endpoint, index) => {
    const params = endpoint.parameters || {};
    const paramTags = Object.entries(params).map(([name, config]) => 
      `<span class="param-tag ${config.optional ? '' : 'required'}">${name}: ${config.type}${config.optional ? ' (optional)' : ''}</span>`
    ).join('');

    return `
      <div class="endpoint-card" data-index="${index}">
        <div>
          <span class="endpoint-method ${endpoint.method}">${endpoint.method}</span>
          <span class="endpoint-route">${endpoint.route}</span>
        </div>
        ${paramTags ? `<div class="endpoint-params">${paramTags}</div>` : ''}
      </div>
    `;
  }).join('');

  // Add click handlers
  container.querySelectorAll('.endpoint-card').forEach(card => {
    card.addEventListener('click', () => {
      const index = parseInt(card.dataset.index);
      selectEndpoint(index);
    });
  });
}

function selectEndpoint(index) {
  selectedEndpoint = endpoints[index];
  
  // Update visual selection
  document.querySelectorAll('.endpoint-card').forEach((card, i) => {
    card.classList.toggle('selected', i === index);
  });

  renderForm();
}

function renderForm() {
  const container = document.getElementById('endpoint-form');
  
  if (!selectedEndpoint) {
    container.innerHTML = '<p class="placeholder">Select an endpoint from the list to test it</p>';
    return;
  }

  const params = selectedEndpoint.parameters || {};
  const paramFields = Object.entries(params).map(([name, config]) => `
    <div class="form-group">
      <label for="param-${name}">
        ${name} 
        <span style="font-weight: normal; color: var(--text-secondary)">
          (${config.type}${config.optional ? ', optional' : ', required'})
        </span>
      </label>
      <input 
        type="${config.type === 'int' || config.type === 'decimal' ? 'number' : 'text'}"
        id="param-${name}"
        name="${name}"
        placeholder="Enter ${name}"
        ${config.type === 'decimal' ? 'step="0.01"' : ''}
        ${!config.optional ? 'required' : ''}
      />
    </div>
  `).join('');

  container.innerHTML = `
    <div class="form-group">
      <label>Endpoint</label>
      <input type="text" value="${selectedEndpoint.method} ${selectedEndpoint.route}" readonly style="background: rgba(99, 102, 241, 0.1);" />
    </div>
    ${paramFields}
    <button class="btn-test" id="btn-test">
      🚀 Test Endpoint
    </button>
  `;

  document.getElementById('btn-test').addEventListener('click', testEndpoint);
}

async function testEndpoint() {
  if (!selectedEndpoint) return;

  const resultsContainer = document.getElementById('results');
  resultsContainer.innerHTML = '<p class="loading">Testing endpoint</p>';

  // Collect parameters
  const params = selectedEndpoint.parameters || {};
  const queryParams = new URLSearchParams();
  
  for (const [name, config] of Object.entries(params)) {
    const input = document.getElementById(`param-${name}`);
    const value = input?.value?.trim();
    
    if (value) {
      queryParams.append(name, value);
    } else if (!config.optional) {
      resultsContainer.innerHTML = `
        <div class="result-error">
          <div class="result-header">
            <span class="result-status error">Validation Error</span>
          </div>
          <pre>Required parameter '${name}' is missing</pre>
        </div>
      `;
      return;
    }
  }

  const url = `${API_BASE_URL}${selectedEndpoint.route}${queryParams.toString() ? '?' + queryParams.toString() : ''}`;
  const startTime = performance.now();

  try {
    const response = await fetch(url, {
      method: selectedEndpoint.method,
      headers: {
        'Accept': 'application/json'
      }
    });

    const endTime = performance.now();
    const duration = (endTime - startTime).toFixed(2);

    let data;
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      data = await response.json();
    } else {
      data = await response.text();
    }

    const isSuccess = response.ok;
    
    resultsContainer.innerHTML = `
      <div class="${isSuccess ? 'result-success' : 'result-error'}">
        <div class="result-header">
          <span class="result-status ${isSuccess ? 'success' : 'error'}">
            ${response.status} ${response.statusText}
          </span>
          <span class="result-time">${duration}ms</span>
        </div>
        <pre>${JSON.stringify(data, null, 2)}</pre>
      </div>
    `;
  } catch (error) {
    const endTime = performance.now();
    const duration = (endTime - startTime).toFixed(2);

    resultsContainer.innerHTML = `
      <div class="result-error">
        <div class="result-header">
          <span class="result-status error">Network Error</span>
          <span class="result-time">${duration}ms</span>
        </div>
        <pre>${error.message}\n\nMake sure the API server is running.</pre>
      </div>
    `;
  }
}

// Initialize the app
renderEndpoints();
renderForm();
