import cv2
import numpy as np
import os
import glob
import os
from PIL import Image
import sys

##########################################################################
##                                                                      ##
##                            Constantes                                ##
##                                                                      ##
##########################################################################

# Path donde buscaremos las imágenes
PATH_ORIGINALES = "./Originales/"

# Path donde se buscarán los patrones
PATH_PATRONES = "./Patrones/"

# Path donde se guardan los archivos marcados con los errores
PATH_MARCADAS = "./Marcadas/"

# Extensión de los patrones
EXTENSION_PATRON = ".jpg"

# Añadido para diferenciar que está marcada
IMG_MARCADA = "_marcada"

######################### FIN CONSTANTES ##################################

"""
    Dado un nombre de archivo (con extensión incluido) comprueba si existe en el directorio
    PATH_ORIGINALES

    Parameters:
    nombre_imagen (string): Nombre del archivo a comprobar su existencia

    Returns:
    bool: True si la imagen existe, False en caso contrario

"""


def existe_imagen(nombre_imagen):

    for f in os.listdir(PATH_ORIGINALES):
        if(f == nombre_imagen):
            return True

    return False


""" 
    Se utiliza una imagen para crear una máscara en función de los píxeles que se detectan como errores
    de textura
 
    Parameters: 
    nombre_imagen (string): Nombre del archivo a procesar
  
    Returns: 
    np Array: Devuelve un array de numpy con la información de la máscara con valores de 0 y 1 dependiendo de si en ese
    píxel se ha detectado un error o no. 

"""


def crea_mascara(nombre_imagen):
    # Por una parte guardamos el nombre sin extensión para ponerlo en la carpeta de marcadas
    nombre_sin_extension = os.path.splitext(nombre_imagen)[0]

    # Por otra parte se guarda la extensión para poder soportar distintas extensiones
    extension_archivo = os.path.splitext(nombre_imagen)[1]

    # Ahora cargamos la imagen encontrada
    img = cv2.imread(PATH_ORIGINALES +
                     nombre_sin_extension + extension_archivo)

    # Se lee el nombre del patron que se quiere utilizar, en función del argumento de entrada
    nombre_patron = sys.argv[2]

    imagen_roja = Image.open(PATH_PATRONES + nombre_patron + EXTENSION_PATRON)
    imagen_roja_rgb = imagen_roja.convert("RGB")
    valores_imagen_roja_rgb = imagen_roja_rgb.getpixel((0, 0))

    # Establecemos el rango mínimo y máximo de (Blue, Green, Red): Dependiendo siempre del patron dado
    azul_altos = np.array(
        [valores_imagen_roja_rgb[2], valores_imagen_roja_rgb[1], valores_imagen_roja_rgb[0]])
    # Establacemos un rango que ayudará en la deteccion (las divisiones idoneas se han encontrado tras la ejecion de varios tets)
    azul_bajos = np.array(
        [(int)(azul_altos[0]/2.23), (int)(azul_altos[1]/8), (int)(azul_altos[2]/2.1)])

    # Recordatorio: el rango sirve para determinar qué píxeles detectaremos.
    # Cada píxel tiene asignado un valor [B, G, R] según su cantidad de Azul, Verde y Rojo.
    # Con el rango que hemos definido, detectaremos los píxeles que cumplan estas tres condiciones:
    #    -Su valor B esté entre 40 y 255
    #    -Su valor G esté entre 0 y 120
    #    -Su valor R esté entre 0 y 120

    # Detectamos los píxeles que estén dentro del rango que hemos establecido:
    mask = cv2.inRange(img, azul_bajos, azul_altos)

    # Guardamos unicamente la imagen marcada
    cv2.imwrite(PATH_MARCADAS + nombre_sin_extension +
                IMG_MARCADA + extension_archivo, mask)


def main():
    # Nombre del archivo que se quiere procesar
    nombre_imagen = sys.argv[1]

    # Si existe el archivo se crea la máscara
    if(existe_imagen(nombre_imagen)):
        crea_mascara(nombre_imagen)

    # En caso contrario salta un error


if __name__ == "__main__":
    main()
