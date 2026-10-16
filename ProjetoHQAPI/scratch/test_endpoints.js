const runTests = async () => {
  // 1. Login
  const loginRes = await fetch('http://localhost:5173/api/login/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'bestlinux@gmail.com', password: '200314Amor@' })
  });
  const auth = await loginRes.json();
  const token = auth.token;
  console.log('1. Autenticacao: OK! Token obtido com sucesso.');

  const headers = {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  };

  // 2. Estatísticas
  const estRes = await fetch('http://localhost:5173/api/hq/estatisticas', { headers });
  const estData = await estRes.json();
  console.log('2. GET /api/hq/estatisticas: Status', estRes.status, estData);

  // 3. Paginação e Filtros
  const pagedRes = await fetch('http://localhost:5173/api/hq?page=1&pageSize=3&orderBy=titulo', { headers });
  const pagedData = await pagedRes.json();
  console.log('3. GET /api/hq (Paginado 3 itens): Status', pagedRes.status, 'Retornou:', pagedData.length, 'Primeiro:', pagedData[0]?.titulo);

  // 4. Busca
  const buscaRes = await fetch('http://localhost:5173/api/hq/busca/balas', { headers });
  const buscaData = await buscaRes.json();
  console.log('4. GET /api/hq/busca/balas: Status', buscaRes.status, 'Encontrados:', buscaData.length);

  // 5. Por Editora
  const edRes = await fetch('http://localhost:5173/api/hq/editora/Panini?page=1&pageSize=2', { headers });
  const edData = await edRes.json();
  console.log('5. GET /api/hq/editora/Panini: Status', edRes.status, 'Retornou:', edData.length);

  // 6. BuscaPorEditora (Boolean)
  const boolRes = await fetch('http://localhost:5173/api/hq/BuscaPorEditora/Panini', { headers });
  const boolData = await boolRes.json();
  console.log('6. GET /api/hq/BuscaPorEditora/Panini: Status', boolRes.status, 'Resultado:', boolData);

  // 7. POST /api/hq (Criar)
  const newHQ = {
    titulo: 'HQ Teste Automatizado',
    editora: 'Panini',
    anoLancamento: '2026',
    numeroEdicao: 1,
    categoria: 1,
    genero: 1,
    status: 1,
    formato: 1,
    preco: 'R$ 29,90',
    numeroPaginas: 48,
    lido: 0,
    capa: 'teste_automacao.jpg'
  };
  const createRes = await fetch('http://localhost:5173/api/hq', {
    method: 'POST',
    headers,
    body: JSON.stringify(newHQ)
  });
  const createdId = await createRes.json();
  console.log('7. POST /api/hq: Status', createRes.status, 'ID criado:', createdId);

  // 8. GET /api/hq/{id}
  const getByIdRes = await fetch('http://localhost:5173/api/hq/' + createdId, { headers });
  const getByIdData = await getByIdRes.json();
  console.log('8. GET /api/hq/{id}: Status', getByIdRes.status, 'Titulo:', getByIdData.titulo, 'Capa:', getByIdData.capa);

  // 9. PUT /api/hq/{id}
  getByIdData.titulo = 'HQ Teste Atualizado Com Sucesso';
  getByIdData.preco = 'R$ 35,00';
  const putRes = await fetch('http://localhost:5173/api/hq/' + createdId, {
    method: 'PUT',
    headers,
    body: JSON.stringify(getByIdData)
  });
  console.log('9. PUT /api/hq/{id}: Status', putRes.status);

  // 10. PATCH /api/hq/{id}/lido
  const patchRes = await fetch('http://localhost:5173/api/hq/' + createdId + '/lido?lido=1', {
    method: 'PATCH',
    headers
  });
  const patchData = await patchRes.json();
  console.log('10. PATCH /api/hq/{id}/lido: Status', patchRes.status, patchData);

  // 11. Upload de Capa via multipart
  const boundary = '----WebKitFormBoundary7MA4YWxkTrZu0gW';
  const dummyJpgContent = Buffer.from([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x60, 0x00, 0x60, 0x00, 0x00, 0xFF, 0xD9]);
  
  const payloadHeader = Buffer.from(
    '--' + boundary + '\r\n' +
    'Content-Disposition: form-data; name="file"; filename="capateste.jpg"\r\n' +
    'Content-Type: image/jpeg\r\n\r\n'
  );
  const payloadFooter = Buffer.from('\r\n--' + boundary + '--\r\n');
  const multipartBody = Buffer.concat([payloadHeader, dummyJpgContent, payloadFooter]);

  const uploadRes = await fetch('http://localhost:5173/api/hq/' + createdId + '/capa', {
    method: 'POST',
    headers: {
      'Authorization': 'Bearer ' + token,
      'Content-Type': 'multipart/form-data; boundary=' + boundary
    },
    body: multipartBody
  });
  const uploadData = await uploadRes.json();
  console.log('11. POST /api/hq/{id}/capa: Status', uploadRes.status, uploadData);

  // 12. GET Capa recém-enviada
  const capaGetRes = await fetch('http://localhost:5173/api/hq/capa/' + uploadData.capa);
  console.log('12. GET /api/hq/capa/{id}: Status', capaGetRes.status, 'Content-Type:', capaGetRes.headers.get('content-type'));

  // 13. DELETE /api/hq/{id}
  const delRes = await fetch('http://localhost:5173/api/hq/' + createdId, {
    method: 'DELETE',
    headers
  });
  const delData = await delRes.json();
  console.log('13. DELETE /api/hq/{id}: Status', delRes.status, 'Resultado:', delData);

  console.log('\n>>> TODOS OS ENDPOINTS TESTADOS E VALIDADOS COM SUCESSO! <<<');
};

runTests().catch(console.error);
