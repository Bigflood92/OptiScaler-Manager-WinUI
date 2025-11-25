# ?? CREAR REPO PRIVADO EN GITHUB - INSTRUCCIONES VISUALES

## ?? Tiempo: 2 minutos

---

## ?? PASO A PASO:

### 1?? Abrir GitHub (nuevo repo)
```
https://github.com/new
```
**Copiar y pegar esa URL en tu navegador** ?

---

### 2?? Configurar el repositorio

En la página que se abre, llenar:

#### Repository name (obligatorio):
```
OptiScaler-Manager-WinUI
```

#### Description (opcional):
```
Modern WinUI 3 version of OptiScaler Manager - Game upscaling mod management (.NET 8)
```

#### Visibility:
**?? IMPORTANTE:** Seleccionar **"Private"** (NO Public)

```
( ) Public
(•) Private  ? SELECCIONAR ESTE
```

#### Initialize repository:
**? DEJAR TODO SIN MARCAR:**

```
[ ] Add a README file
[ ] Add .gitignore
[ ] Choose a license
```

---

### 3?? Crear repositorio

Click en el botón verde grande:
```
[Create repository]
```

---

### 4?? Copiar URL

Después de crear, GitHub te mostrará una página con comandos.

**COPIAR** la URL HTTPS que aparece arriba (será algo como):

```
https://github.com/Bigflood92/OptiScaler-Manager-WinUI.git
```

**Esa es la URL que necesitas!** ??

---

## ? VERIFICACIÓN

Tu pantalla debe mostrar algo como:

```
Quick setup — if you've done this kind of thing before

HTTPS [SSH]
https://github.com/Bigflood92/OptiScaler-Manager-WinUI.git

…or create a new repository on the command line
...
```

**Copia esa URL HTTPS!**

---

## ?? DESPUÉS DE COPIAR LA URL:

Vuelve a Visual Studio / PowerShell y pega la URL cuando te lo pida.

El script automáticamente hará:
- ? Agregar remote a Git
- ? Push inicial
- ? Configurar tracking

---

## ?? SI TIENES PROBLEMAS:

### Error: "Repository already exists"
- Ya creaste un repo con ese nombre antes
- Opciones:
  - A) Usar nombre diferente: `OptiScaler-Manager-WinUI-v2`
  - B) Eliminar el repo viejo y crear uno nuevo

### Error: "Authentication failed"
- Necesitas **Personal Access Token** (no contraseña)
- Ve a: https://github.com/settings/tokens
- Generate new token (classic)
- Scopes: `repo` (full control)
- Usa el token como contraseña cuando Git lo pida

---

## ?? DESPUÉS DE GITHUB PAGES:

Una vez el repo esté creado y GitHub Pages activado:

1. **Actualizar manifest** con URL final
2. **Optimizar PNG** con TinyPNG
3. **Capturar screenshots**
4. **Testing navegación**
5. **Submit a Store!**

---

**¿LISTO? Abre GitHub y crea el repo! ??**
