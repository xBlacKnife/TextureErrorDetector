import cv2
import numpy as np
import os
import glob
import os
from PIL import Image, ImageOps
import sys
import json

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

# Extensión de los .json
EXTENSION_JSON = ".json"

# Añadido para diferenciar que está marcada
IMG_MARCADA = "_marcada"

# Color en formato BGR con el que se repintan los píxeles que dan error
COLORBGR_SOBREPINTADO = [255, 255, 255]

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


'''
    Se utiliza la mascara obtenida para generar un archivo en formato json con las coordenadas de los pixeles que se han 
    detectado como error

    Parameters:
    nombre (string): Nombre del archivo a procesar
    shape0 (int): Alto de la foto
    shape1 (int): Ancho de la foto
    mascara (array): Array de 0s y 1s con la info de los pixeles que son un error
    
'''


def guardar_json(nombre, shape0, shape1, mascara):
    pixeles_marcados = {}
    pixeles_marcados[nombre]={}
    pixeles_marcados[nombre]['Pixeles_Marcados'] = []
    pixeles_marcados[nombre]['Porcentaje_Fallo'] = calcular_porcentaje_fallo(mascara)
    for i in range(0, shape0):
        for j in range(0, shape1):
            if(mascara[i][j]):
                coordenadas = {}
                coordenadas['x'] = i
                coordenadas['y'] = j
                pixeles_marcados[nombre]['Pixeles_Marcados'].append(coordenadas)

    if not os.path.exists('texture_error' + EXTENSION_JSON):
        with open('texture_error' + EXTENSION_JSON, 'w') as outfile:
            json.dump({}, outfile, indent=2)

    with open('texture_error' + EXTENSION_JSON) as outfile:
        data = json.load(outfile)

    data.update(pixeles_marcados)
    
    with open('texture_error' + EXTENSION_JSON, 'w') as outfile:
        json.dump(data, outfile, indent=2)

'''
    Se utiliza la imagen original y la máscara para pintar encima de la imagen original los píxeles que 
    son detectados como error

    Parameters:
    img (string): Array con el contenido de los pixeles en formato BGR de la foto original
    mascara (array): Array de 0s y 1s con la info de los pixeles que son un error

'''


def marcar_coloreando_encima(img, mascara):
    for i in range(0, img.shape[0]):
        for j in range(0, img.shape[1]):
            if(mascara[i][j]):
                img[i][j] = COLORBGR_SOBREPINTADO

    cv2.imwrite(PATH_MARCADAS + "_coloreadoencima" +
                IMG_MARCADA + ".jpg", img)

'''
    Se utiliza la imagen original y la máscara para calcular el porcentaje de pixeles que se han visto
    afectados y saber así, si existe o no alguna textura fallida

    Parameters:
    img (string): Array con el contenido de los pixeles en formato BGR de la foto original
    mascara (array): Array de 0s y 1s con la info de los pixeles que son un error

'''

def calcular_porcentaje_fallo(mascara):
    total=0
    fallo=0
    for i in range(0, mascara.shape[0]):
        for j in range(0, mascara.shape[1]):
            if(mascara[i][j]):
                fallo+=1
            total+=1
    return((fallo/total)*100)



'''
    Se utiliza la imagen original y la máscara para eliminar en la imagen original los píxeles que 
    son detectados como error

    Parameters:
    nombre (string): Nombre de la foto original
    extension (string): Extensión de la foto original
    mascara (array): Array de 0s y 1s con la info de los pixeles que son un error

'''


def marcar_eliminando_pixeles(nombre, extension, mascara):

    img_rgb = Image.open(PATH_ORIGINALES +
                         nombre + extension)
    img_alpha = Image.open(PATH_MARCADAS + nombre +
                           "_mascara" + extension).convert('L').resize(img_rgb.size)

    img_alpha = ImageOps.invert(img_alpha)

    img_rgb.putalpha(img_alpha)
    img_rgb.save(PATH_MARCADAS + nombre +
                 "_eliminandopixeles" + ".png")


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
    nombre_patron = sys.argv[1]

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
    mascara = cv2.inRange(img, azul_bajos, azul_altos)

    # Guardamos unicamente la imagen marcada
    cv2.imwrite(PATH_MARCADAS + nombre_sin_extension +
                "_mascara" + extension_archivo, mascara)

    #marcar_coloreando_encima(img, mascara)
    #marcar_eliminando_pixeles(nombre_sin_extension, extension_archivo, mascara)
    guardar_json(nombre_sin_extension, img.shape[0], img.shape[1], mascara)


def main():

    imag_list = os.listdir(PATH_ORIGINALES)

    for nombre_imagen in imag_list:
        # Si existe el archivo se crea la máscara
        if(existe_imagen(nombre_imagen)):
            crea_mascara(nombre_imagen)

        # En caso contrario salta un error
        else:   
            raise FileNotFoundError(
                "No se ha encontrado el archivo " + nombre_imagen + " en la carpeta " + 'Originales')


if __name__ == "__main__":
    main()
