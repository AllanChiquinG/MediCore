# Plan de pruebas — MediCore

**Proyecto:** MediCore, plataforma hospitalaria y de atención médica
**Curso:** 048 Aseguramiento de la Calidad del Software (UMG)
**Versión:** 0.1 (borrador) · **Fecha:** 2026-10-08
**Documento relacionado:** [Matriz de riesgos](matriz-de-riesgos.md)

## 1. Objetivo
Verificar que MediCore cumple sus requisitos funcionales y de calidad, protege la información clínica, soporta la demanda esperada y se recupera de fallas de Redis y RabbitMQ, para emitir una decisión **GO / NO-GO** basada en evidencia.

## 2. Alcance

**Incluido**
- Gestión de pacientes y médicos
- Agenda y reserva de citas
- Consultas médicas
- Recetas
- Registro y consulta de resultados de laboratorio
- Notificaciones al paciente (flujo asíncrono con RabbitMQ)
- API REST, frontend web y aplicación móvil para pacientes
- Cache en Redis y persistencia en PostgreSQL

**Fuera de alcance**
- Integraciones con sistemas hospitalarios reales
- Facturación y seguros
- Datos reales de pacientes (solo datos de prueba)

## 3. Niveles y tipos de prueba

| Tipo | Qué valida | Herramienta |
|------|------------|-------------|
| Unitarias | Reglas de negocio (horarios, solapamientos, estados de cita) | xUnit |
| Integración | API + PostgreSQL + Redis + RabbitMQ | xUnit + contenedores |
| API | Casos positivos, negativos y de autorización | Insomnia |
| Funcionales web / regresión | Flujos de paciente, médico y administrador | Cypress |
| Móvil | Solicitar y consultar citas, ver resultados y recetas, notificaciones | Maestro |
| Seguridad (SAST/SCA) | Código y dependencias | SonarQube, Fortify, Dependency-Check, npm audit |
| Seguridad (DAST) | Aplicación en ejecución | OWASP ZAP |
| Rendimiento | Carga, estrés y pico | k6 o JMeter |
| Concurrencia | Doble reserva y colisión de horarios | k6 o JMeter + pruebas de integración |
| Resiliencia | Caída de Redis y de RabbitMQ | Docker Compose (detener servicios) |
| Usabilidad y accesibilidad | Experiencia y cumplimiento de accesibilidad | Pruebas con usuarios, Lighthouse o axe |

## 4. Ambientes
- **Local:** desarrollo con Docker Compose (PostgreSQL, Redis, RabbitMQ).
- **QA:** ambiente reproducible desplegado por el pipeline en Jenkins.
- Datos: únicamente datos de prueba inventados.

## 5. Roles de usuario a probar
Administrador, Médico, Paciente y Laboratorio. Cada caso de prueba indica el rol con el que se ejecuta.

## 6. Casos de prueba por área (resumen inicial)

| ID | Área | Caso | Rol | Resultado esperado | Riesgo |
|----|------|------|-----|--------------------|--------|
| CP-01 | Citas | Reservar una cita en un horario libre | Paciente | Cita creada y evento `AppointmentCreated` publicado | R07 |
| CP-02 | Citas | 50 solicitudes simultáneas para el mismo horario | Paciente | Exactamente 1 cita creada; el resto rechazadas | R07, R08 |
| CP-03 | Citas | Reservar en horario solapado con otra cita del médico | Paciente | Rechazo con error claro | R08 |
| CP-04 | Citas | Cancelar una cita | Paciente | Horario liberado y caché invalidada | R12 |
| CP-05 | Autorización | Consultar `GET /api/patients/{id}` de otro paciente | Paciente | 403 Forbidden | R01 |
| CP-06 | Autorización | Consultar resultados de laboratorio | Administrativo | Acceso denegado | R02 |
| CP-07 | Autenticación | Usar un token vencido o manipulado | Cualquiera | 401 Unauthorized | R04 |
| CP-08 | Laboratorio | Registrar resultado con RabbitMQ detenido | Laboratorio | Resultado guardado en BD; notificación enviada al reiniciar RabbitMQ | R13 |
| CP-09 | Recetas | Consultar `GET /api/prescriptions/{id}` propia | Paciente | Receta devuelta con campos esperados | R10 |
| CP-10 | Redis | Consultar disponibilidad dos veces (MISS y luego HIT) | Paciente | Segunda respuesta desde caché | R12 |
| CP-11 | Redis | Esperar vencimiento del TTL | Paciente | Dato actualizado desde BD | R12 |
| CP-12 | Redis | Detener Redis y consultar disponibilidad | Paciente | Respuesta correcta desde BD, sin error 500 | R14 |
| CP-13 | Seguridad | Enviar cargas de inyección SQL y XSS | Cualquiera | Entrada rechazada o neutralizada | R06 |
| CP-14 | Seguridad | Revisar respuestas y logs por datos sensibles | — | Sin datos clínicos innecesarios expuestos | R03 |
| CP-15 | Rendimiento | Pacientes solicitando citas mientras médicos consultan agendas | Mixto | Cumple umbrales de la sección 7 | R15 |
| CP-16 | Móvil | Solicitar cita y consultar resultado desde la app | Paciente | Flujo completo sin errores (Maestro) | R18 |

Este listado se ampliará con casos detallados (pasos, datos y evidencia) en `docs/casos-de-prueba.md`.

## 7. Métricas y umbrales (propuestos)

| Métrica | Umbral |
|---------|--------|
| Latencia p95 en consulta de disponibilidad | ≤ 500 ms |
| Latencia p95 al reservar cita | ≤ 1000 ms |
| Tasa de error bajo carga normal | < 1 % |
| Citas duplicadas bajo concurrencia | 0 |
| Pérdida de datos clínicos con RabbitMQ caído | 0 |
| Cobertura de pruebas unitarias | ≥ 70 % en lógica de negocio |
| Vulnerabilidades críticas sin aceptar | 0 |
| Quality Gate de SonarQube | Aprobado |

Los umbrales se ajustarán al medir la línea base real.

## 8. Criterios de entrada y salida

**Entrada:** compilación exitosa, ambiente QA desplegado, datos de prueba cargados, casos de prueba aprobados.

**Salida (GO):** sin defectos críticos abiertos, todas las pruebas críticas aprobadas, Quality Gate aprobado, sin vulnerabilidades críticas sin aceptar, umbrales de rendimiento cumplidos.

**NO-GO:** una prueba crítica fallida, una vulnerabilidad crítica no aceptada o un Quality Gate rechazado.

## 9. Gestión de defectos
Los defectos se registran como Issues en GitHub con: título, pasos para reproducir, resultado esperado y obtenido, severidad (crítica, alta, media, baja), evidencia (captura o log) y estado. Etiquetas: `bug`, `seguridad`, `rendimiento`, `accesibilidad`.

## 10. Evidencias y reportes
Se guardan en `docs/evidencias/` y `docs/reportes/`: reportes de SonarQube, Fortify, Dependency-Check, npm audit y ZAP; resultados de Cypress, Maestro y k6/JMeter; colección de Insomnia; capturas de la cola y de los consumidores procesando eventos.

## 11. Cronograma
Pendiente de definir con el equipo según las fechas de entrega del curso.
