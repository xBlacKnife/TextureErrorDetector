import cv2
import numpy as np
import os
import glob, os
from PIL import Image

#path donde buscaremos las imaganes
path="./Originales"

#Primero buscamos el nombre del archivo en la imagen
archivoImagen = next(os.path.join(path, f) for f in os.listdir(path) if os.path.isfile(os.path.join(path, f)))

#Guardamos el nombre sin la extension que usaremos para ponerlo en la carpeta de marcadas
nombre_imagen=os.path.splitext(archivoImagen)[0]
nombre_imagen=nombre_imagen.split('\\')[1]
print (nombre_imagen)

#Ahora cargamos la imagen encontrada
img = cv2.imread("./Originales/"+nombre_imagen+".jpg")

red_image = Image.open("Patrones/unity.jpg")
red_image_rgb = red_image.convert("RGB")
rgb_pixel_value = red_image_rgb.getpixel((0,0))

print(rgb_pixel_value[0])


#Establecemos el rango mínimo y máximo de (Blue, Green, Red): Dependiendo siempre del patron dado
azul_altos = np.array([rgb_pixel_value[2],rgb_pixel_value[1],rgb_pixel_value[0]])
#Establacemos un rango que ayudará en la deteccion (las divisiones idoneas se han encontrado tras la ejecion de varios tets)
azul_bajos = np.array([(int)(azul_altos[0]/2.23),(int)(azul_altos[1]/8),(int)(azul_altos[2]/2.1)])


# Recordatorio: el rango sirve para determinar qué píxeles detectaremos.
# Cada píxel tiene asignado un valor [B, G, R] según su cantidad de Azul, Verde y Rojo.
# Con el rango que hemos definido, detectaremos los píxeles que cumplan estas tres condiciones:
# -Su valor B esté entre 40 y 255
# -Su valor G esté entre 0 y 120
# -Su valor R esté entre 0 y 120
 
 
#Detectamos los píxeles que estén dentro del rango que hemos establecido:
mask = cv2.inRange(img, azul_bajos, azul_altos)

#Guardamos unicamene la imagen marcada
cv2.imwrite("Marcadas/"+nombre_imagen+"-marcada.jpg", mask) 
 
#Salimos pulsando cualquier tecla:
#print (os.path.split(s)[-1]).split(s)[0]