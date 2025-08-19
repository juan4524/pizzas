// NodeScripts/basedatos.js
const { MongoClient } = require('mongodb');

let cliente;

async function obtenerCliente(uri) {
  if (!cliente) cliente = new MongoClient(uri, { ignoreUndefined: true });
  if (!cliente.topology || cliente.topology.isClosed()) await cliente.connect();
  return cliente;
}

// PRUEBA: ping a la BD (para verificar conexión real)
module.exports.pingBD = async (entrada) => {
  const { mongoUri, nombreBD } = entrada;
  const cli = await obtenerCliente(mongoUri);
  const db = cli.db(nombreBD);
  await db.command({ ping: 1 });
  return { ok: true, mensaje: "PONG desde MongoDB" };
};

// PRUEBA: inserta un documento de prueba
module.exports.insertarPrueba = async (entrada) => {
  const { mongoUri, nombreBD, coleccion } = entrada;
  const cli = await obtenerCliente(mongoUri);
  const db = cli.db(nombreBD);
  const col = db.collection(coleccion);
  const doc = { creado_en: new Date(), nota: "documento de prueba" };
  const r = await col.insertOne(doc);
  return { ok: true, id: r.insertedId.toString() };
};