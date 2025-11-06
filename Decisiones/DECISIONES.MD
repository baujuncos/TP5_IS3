# TP5: Release Pipelines
### Alumnos: Juncos Bautista y Treachi Belén.

[RepoEnAzure-TP5-JUNCOS-TREACHI](https://baujuncos@dev.azure.com/baujuncos/TP5_IS3_2doIntento/_git/TP5_IS3_2doIntento)

## Configuramos los recursos que usaremos:

1. Creamos la Web App de QA y de PROD en Azure Portal con las siguientes especificaciones:

* Crear Resource Group: rg-tp05-ingsoft3-2025
* Crear App Service Plan: plan-tp05-free
* Crear Web App QA: webapp-tp05-qa-juncos-treachi
* Runtime: NODE 20 LTS (long term support)
* OS: Linux (porque en teoria Node funciona mejor en Linux)
* Publish: Code


Ejemplo con PROD (análogo para QA)
![alt text](img.png)
![alt text](image-2.png)


Así queda nuestro grupo de recursos con todo en ejecución y listo para los deployments:
![alt text](image-3.png)

## Configuramos una Service Connection en Azure DevOps

Esto va a actuar como un canal de comunicación seguro entre el pipeline de Azure DevOps y nuestros recursos externos (como Azure en este caso, pero también podría ser Amazon Web Services, GitHub, o un servidor genérico).

1. Dentro del proyecto de Azure donde estamos haciendo el TP me dirijo a Project Settings.
2. Click en "Service connections" > Click "New service connection" > "Azure Resource Manager" > "Service principal (automatic)"
* Subscription: Seleccionamos nuestra suscripción Azure que es Azure for Students
* Resource Group: rg-tp05-ingsoft3-2025
* Service connection name: azure-tp05-connection
* Grant access permission to all pipelines: ✅

![alt text](image-4.png)
![alt text](image-5.png)

## Pipeline YAML básico:
Acá creamos el archivo azure-pipelines.yml donde desarrollaremos la escritura del build y los deployments utilizando nuetros App Services y nombrando las variables de entorno pertinentes según avancemos.

*[FIJARSE EN PIPELINE LA STAGE BUILD]*

### Verificamos que funciona el pipeline y que publica los artefactos con éxito:

**Drop con app perfectamente publicada:**

![alt text](image-6.png)

**En nuestro caso comprime todo en un zip:**

![alt text](image-7.png)

**Abrimos el zip y descubrimos...**

![alt text](image-8.png)

## Extendemos el pipeline para incluir deploy a QA:

* Antes que nada creamos los entornos en la sección "Environments" de los Pipelines:

![alt text](image-9.png)

* **Siguiendo con el desarrollo del primer deployment (QA):**

Acá ya encontramos muchos mas conflictos, más que nada con la database y porque tenemos que configurar variables de entorno que son necesarias para trabajar con nuestra base de datos SQLITE. Por lo tanto comenzamos el deployment en nuestra app de QA y nos topamos con que el frontend se entrega con éxito pero no nos permite modificar la base de datos, es decir, no podemos crear nuevas tasks, editarlas, eliminarlas, etc. Lo mismo pasa con el registro de usuarios y así.

Esto es porque la DB se encontraba en un directorio raíz y como nuestra AppService esta trabajando en Linux el archivo a estar en tal directorio lo configuraba automáticamente como READ-ONLY. Así que tuvimos que modificar el donde se desarrolla el SQLITE y relegarlo a subcarpetas donde si fuera modificable. Implementamos variable de entorno DATABASE_PATH

Previamente, al hacer inicio de sesión nos daba error porque nos faltaba implementar una variable de entorno: JWT_SECRET. Este error viene del módulo jsonwebtoken y significa que cuando estamos intentando generar un token JWT, no estamos proporcionando una clave secreta (secret) válida.

* **Configuración de variables de entorno en QA (análogo en producción posteriormente):**

![alt text](image-14.png)


* **Ejecución del pipeline:**

![alt text](image-12.png)
![alt text](image-13.png)


*  **Éxito final en QA:**

![alt text](image-10.png)
![alt text](image-11.png)

## Agregamos stage de deployment a PROD:

*  **Actualizamos azure-pipelines.yml para incluir PROD:**


* **Ejecución del pipeline:**

![alt text](image-19.png)
![alt text](image-16.png)


*  **Éxito final en PROD:**

Para verificar de manera simple si estamos en PROD o en QA lo que hacemos es crear un usuario que se llama "estoyenprod" con contraseña "produccion" que no existe en QA y si en PROD.

![alt text](image-17.png)
![alt text](image-18.png)


## Configuramos aprobación manual para PROD:

El objetivo es agregar una aprobación manual que debe realizar el developer para que se ejecute la parte de deployment en PROD como medida de seguridad y prevensión de errores.

**Pasos:** 

* Ir a Pipelines > Environments > PROD
* Click en los tres puntos (...) > Approvals and checks
* Click "+" > Approvals
* Añadir usuarios que pueden aprobar (tu usuario)
* Timeout: 30 días
* Minimum approvers: 1
* Save

![alt text](image-20.png)

![alt text](image-21.png)


**Nos fijamos si nos pide aprobacion de Run cuando ejecutamos el pipeline de nuevo:**

* Pipeline ejecuta Build ✅
* Deploy a QA se ejecuta automáticamente ✅
* Deploy a PROD queda pendiente de aprobación ⏳

![alt text](image-22.png)

![alt text](image-23.png)

**Aprobamos despliegue en PROD:**

![alt text](image-24.png)


## Aplicamos mini cambio en estilos css para ver como afecta al flujo de deployment QA vs PROD

Cambiamos una linea del body del styles.css:

Original: background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);


Nueva:     background: linear-gradient(135deg, #667eea 0%, #21a407 100%);


### QA y PROD antes de cambio de color de fondo en el codigo:

**QA**
![alt text](image-25.png)

**PROD**
![alt text](image-26.png)

### Corremos el pipeline y notamos que QA tiene el nuevo color de fondo y PROD todavia no porque no aprobamos su despliegue todavia, asi que sigue con el de antes:

**QA**
![alt text](image-27.png)

**PROD**: notamos que PROD sigue con los colores viejos
![alt text](image-28.png)

### Aprobamos el deployment en PROD y vemos si los colores de PROD se actualizan correctamente:

![alt text](image-29.png)

Colores actualizados con éxito:
![alt text](image-30.png)

Vuelvo a mis colores de antes que me gustan m+as:
![alt text](image-31.png)